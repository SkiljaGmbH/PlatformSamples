using SampleActivities.Properties;
using STG.Common.DTO;
using STG.Common.DTO.Configuration;
using STG.Common.DTO.Licensing;
using STG.Common.DTO.Metadata;
using STG.Common.Interfaces.Activities;
using STG.Common.Interfaces.Security;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.RT.API.Document.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SampleActivities
{
    public class LicensedActivityWithRequiredVolume : STGUnattendedAbstract<LicensedActivityWithRequiredVolume.Config>, ISTGLicensedActivity
    {
        private string _secureToken;
        private DtoActivityLicenseData _license;
        private ILicenseVerifier _verifier;
        private DtoActivityLicenseEntry _licenseEntry;

        /// <summary>
        /// ISTGLicensedActivity.SkipLicenseValidation
        /// With this mechanism, activity license verification is completely in the hands of the implementer.
        /// This can be used if activities should be able to run without a license - or to just limit certain features to just work with a license
        /// </summary>
        public bool SkipLicenseValidation => true;

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
            VerifyLicense();
            VerifyExistingVolume();

            var customValueText = "executed at " + DateTime.UtcNow.ToString("u");
            documentToProcess.AddCustomValue("Activity is processing", customValueText, false);

           
            // We simply charge on the document, every time that we encounter the document. Using this method, we're not able to tell if we already charged a license on this document or not.
            // via the extension method AddUsedVolume(Guid, string, int), so that volume is only persisted by the platform when saving of a document succeeds.
            // All extension methods for the STGDocument and its elements are found in the STG.RT.API.Document.Licensing namespace
            documentToProcess.AddUsedVolume(GetLicenseId(), Config.RequiredVolumeName, 1);

        }

        /// <summary>
        /// Demonstrates how to check on used volume and how to compare it with the volume data that is signed and given by the activity vendor
        /// </summary>
        private void VerifyExistingVolume()
        {
            /* WHO ENFORCES THE VOLUME LIMIT?
             * The platform itself is responsible for the platform volume: that's work items, documents, media and pages, irrespective of activities used.
             * The activity volume is the responsibility of the activity. The platform keeps track of what is used, but it does not tell 'stop'.
             * The activity itself must decide, whether the Used volume is enough to process the given work item, or not.
             * Note, that the Used counter is not a real-time view onto used counters.
             */

            // the activity license entry is the part that's maintained by the platform, and thus allows us to count volume for you, while keeping the original signed token valid.
            // when the activity license is registered with the platform the _licenseEntry.Volumes are populated from the signed license token (_licenseEntry.GetActivityLicense().Volumes).
            // So the only difference of both volumes lists is that the list from _licenseEntry can count used volume, while the other list doesn't.
            var volumeCountedViaSystem = _licenseEntry.Volumes;

            /* A NOTE ON GRACE VOLUMES:
             * the GraceVolume property is per se just an Int64. The platform grace volume, as well as the activities that we touched, use grace as a percentage.
             * Hence, the platform's License Management View shows a percentage sign behind grace volumes, and calculates grace volumes everywhere by adding the grace in % on top of the max volume.
             * When license volume is renewed with the new year, the new Used volume is taken as Math.Max(lic.Used - lic.MaxVolume, 0).
            */

            // let's check that the activity provides the required volume it the license.
            var requiredVolume = volumeCountedViaSystem?.FirstOrDefault(x => x.Name.Equals(Config.RequiredVolumeName, StringComparison.OrdinalIgnoreCase));
            if (requiredVolume == null)
            {
                Log.Debug($"The LicensedActivityWithRequiredVolumeSample does not have volume for {Config.RequiredVolumeName}.");
                throw new STG.Common.Utilities.Exceptions.STGLicenseException(STG.Common.Utilities.Exceptions.ErrorCode.LicenseMissingVolume, $"The activity license volume '{Config.RequiredVolumeName}' is not present on the activity license ({this.GetLicenseId()})");
            }
            else if (requiredVolume.Used >= requiredVolume.MaxVolume + requiredVolume.MaxVolume * (requiredVolume.GraceVolume / 100.0))
            {
                Log.Debug($"The volume {Config.RequiredVolumeName} on LicensedActivityWithRequiredVolumeSample has already exceeded the allowed amount.");
                throw new STG.Common.Utilities.Exceptions.STGLicenseException(STG.Common.Utilities.Exceptions.ErrorCode.LicenseMissingVolume, $"The activity license volume '{Config.RequiredVolumeName }' is exceeded");
            }
        }

        /// <summary>
        /// Verify the activity license in the activity itself
        /// </summary>
        private void VerifyLicense()
        {
            // throwing an exception is the preferred way to tell the platform that something is wrong.
            // Remember: activities should not ever need to log a warning or error message. Failures are communicated by throwing an exception
            if (_secureToken == null)
            {
                throw new Exception($"No activity license received for activity type {ActivityInfo.ActivityTypeName}");
            }
            if (_verifier.VerifySignature(_secureToken, GetPublicKey()) == false)
                throw new Exception("The activity license is not valid - signature is not valid");
            if (_license.ExpirationDate < DateTime.UtcNow)
                throw new Exception($"The activity license is not valid - it already expired on {_license.ExpirationDate}");
        }

        /// <summary>
        /// Called by the activity host every time before an activity is executed.
        /// It provides the activity license (if it exists, <c>null</c> otherwise), and a <paramref name="licenseVerifier"/> to verify the license signature.
        /// If the <paramref name="licenseEntry"/> is <c>null</c>, then the license was either retracted or never on the environment.
        /// </summary>
        /// <param name="licenseEntry">The activity license with platform metadata. Can be <c>null</c> if there was no license found on the platform, for example when the license was retracted to design-time.</param>
        /// <param name="licenseVerifier">Provides a method to verify the signature of the activity license</param>
        public void SetLicenseData(DtoActivityLicenseEntry licenseEntry, ILicenseVerifier licenseVerifier)
        {
            if (licenseEntry == null)
            {
                // if the licenseEntry is null, the license is either not there or has been retracted to design-time.
                // Make sure your activity reacts properly to that situation
                Log.Debug("No activity license present");
                _license = null;
                return;
            }

            _licenseEntry = licenseEntry;
            _secureToken = licenseEntry.LicenseToken;
            // licenseEntry is the shell object that contains the activity license that the activity vendor generated.
            // GetActivityLicense is a convenience method to deserialize the DtoSecureToken in licenseEntry.License
            try
            {
                _license = licenseEntry.GetActivityLicense();
            }
            catch (Exception ex)
            {
                Log.Debug("The activity license couldn't be read", ex);
                _license = null;
                return;
            }
            // licenseVerification contains a class that takes the DtoActivityLicenseEntry.License and the public key to verify that license.
            _verifier = licenseVerifier;
        }

        /// <summary>
        /// Called by the activity host after an activity is initialized.
        /// It is used to verify that the activity license is valid, unless SkipLicenseValidation is set to true.
        /// </summary>
        /// <returns>A DER or PEM certificate with the public key</returns>
        public byte[] GetPublicKey()
        {
            return Resources.sampleActivityPublic;
        }

        /// <summary>
        /// Called by the activity host so that the activity license can be loaded and set.
        /// This guid is vendor generated and should be unique (at least to this vendor)
        /// </summary>
        public Guid GetLicenseId()
        {
            return Guid.Parse("2b93e963-87ab-484b-8eb3-9fc311bb2166");
        }

        public class Config : ActivityConfigBase<Config>, IActivityConfigurationValidation
        {

            internal static string RequiredVolumeName = "RequiredDocumentVolume";
            public Config()
            {
                PlatformDataDesc = "This activity demonstrates how to check if the required license or the counter is present on the registered license(s). If a license is given, it will write the custom value " +
                    "'Activity is processing' to the document. " +
                    "The activity requires a license to be present on runtime. If the license is not present, activity execution fails. " +
                    "The activity also requires a license counter '" + RequiredVolumeName + "' to be present on the license. If the counter is not there execution fails. " +
                    "It is assumed that counter has enough volume. If all the counter is used, execution fails. " +
                    "Finally, the activity allows validation of the settings, and warns about missing license or counter on the activity.";
            }

            [Display(Name = "Licensed Activity With Required Volume", Description = "Showcases how to check for a required License volume and how to implement license validation and consumption.")]
            [InputType(InputType.textarea), ReadOnly(true)]
            public string PlatformDataDesc { get; set; }

            public IList<DtoActivityConfigurationValidationResult> Validate(DtoActivityConfigurationValidationOptions options)
            {
                var ret = new List<DtoActivityConfigurationValidationResult>();
                if (options.IsDesigntimeEnvironment)
                {
                    if (options.ActivityLicenses == null || options.ActivityLicenses.Count == 0)
                    {
                        //If no valid license exists on the DT warn the user (This is warning because user can still configure the process even if license is not present)
                        ret.Add(new DtoActivityConfigurationValidationResult
                        {
                            Level = DtoActivityConfigurationValidationLevel.Warning,
                            PropertyName = "MissingLicense",
                            Message = $"The activity requires a license to be able to work, but your design-time has no required license(s) registered."
                        });
                    }
                    else
                    {
                        //In design-time we get all the registered licenses that matches the License ID for the current activity. List a warning for every license that has no required counter
                        foreach (var lic in options.ActivityLicenses)
                        {
                            var requiredCounter = lic.Volumes?.FirstOrDefault(v => v.Name.Equals(RequiredVolumeName, StringComparison.OrdinalIgnoreCase));
                            if (requiredCounter == null)
                            {
                                ret.Add(new DtoActivityConfigurationValidationResult
                                {
                                    Level = DtoActivityConfigurationValidationLevel.Warning,
                                    PropertyName = "MissingLicenseCounter",
                                    Message = $"The activity requires a license with a volume named '{RequiredVolumeName}' to be able to work, but the license with name '{lic.LicenseName}' and ID '{lic.Id}' has no volume with that name."
                                });
                            }
                            //We won't warn about missing volume in the design-time
                        }
                    }
                }
                else
                {
                    //On runtime we report an error if license is not deployed to runtime
                    if (options.ActivityLicenses == null || options.ActivityLicenses.Count == 0)
                    {
                        ret.Add(new DtoActivityConfigurationValidationResult
                        {
                            Level = DtoActivityConfigurationValidationLevel.Error,
                            PropertyName = "MissingLicense",
                            Message = $"The activity requires a license to be able to work, but the license required for this activity is not deployed on the current runtime environment."
                        });
                    }
                    else
                    {
                        //On runtime environment we get only license that is deployed to that runtime, and there can be only one
                        var deployedLicense = options.ActivityLicenses.First();
                        //if the counter is missing we report a validation error
                        var requiredCounter = deployedLicense.Volumes?.FirstOrDefault(v => v.Name.Equals(RequiredVolumeName, StringComparison.OrdinalIgnoreCase));
                        if (requiredCounter == null)
                        {
                            ret.Add(new DtoActivityConfigurationValidationResult
                            {
                                Level = DtoActivityConfigurationValidationLevel.Error,
                                PropertyName = "MissingLicenseCounter",
                                Message = $"The activity requires a license with a volume named '{RequiredVolumeName}' to be able to work, but the license with name '{deployedLicense.LicenseName}' and ID '{deployedLicense.Id}' has no such volume."
                            });
                        }
                        else
                        {
                            //We also report error if counters on the license are exceeded
                            if (requiredCounter.Used >= requiredCounter.MaxVolume + requiredCounter.MaxVolume * (requiredCounter.GraceVolume / 100.0))
                            {
                                ret.Add(new DtoActivityConfigurationValidationResult
                                {
                                    Level = DtoActivityConfigurationValidationLevel.Error,
                                    PropertyName = "LicenseCounterExceeded",
                                    Message = $"The activity license volume '{RequiredVolumeName}' has already exceeded the allowed amount."
                                });
                            }
                            else if (requiredCounter.Used >= requiredCounter.MaxVolume) //We also report warning if license used all the volume but has Grace left
                            {
                                ret.Add(new DtoActivityConfigurationValidationResult
                                {
                                    Level = DtoActivityConfigurationValidationLevel.Warning,
                                    PropertyName = "LicenseCounterUsesGrace",
                                    Message = $"The activity license volume '{RequiredVolumeName}' has exceeded the Max Volume and uses Grace."
                                });
                            }
                        }
                    }
                }

                return ret;
            }
        }
    }
}
