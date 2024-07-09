using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using SampleActivities.Properties;
using STG.Common.DTO;
using STG.Common.DTO.Licensing;
using STG.Common.DTO.Metadata;
using STG.Common.Interfaces.Security;
using STG.RT.API;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.RT.API.Document.Licensing;

namespace SampleActivities
{
    public class LicensedActivityWithVolume : STGUnattendedAbstract<LicensedActivityWithVolume.Config>, ISTGLicensedActivity
    {
        /// <summary>
        /// This is a random byte[64] array that we store as a base64 string.
        /// It's used to be able to verify that activity volume was consumed by this activity and not by another activity that jus says that it was us.
        /// It was generated with:
        /// var random = new Random();
        /// var b = new byte[64];
        /// random.NextBytes(b);
        /// string base64String = Convert.ToBase64String(b);
        /// </summary>
        private byte[] _secretHashKey =
            Convert.FromBase64String(
                "nbUK3T1TVZIlqgTyf3euJlXIgeoeJXZ1btKxKNiYZHImEFM6+dgJeE+dAUOnWplM3eiFriz0kXc+lW9roi25jA==");
        private string _secureToken;
        private DtoActivityLicenseData _license;
        private MyLicense _myLicense;
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

            if (_myLicense.Feature1)
            {
                documentToProcess.AddCustomValue("Feature 1", customValueText, false);
            }

            if (_myLicense.Feature2)
            {
                documentToProcess.AddCustomValue("Feature 2", customValueText, false);
                documentToProcess.AddUsedVolume(GetLicenseId(), "feature_2_volume", 1);
            }

            // There are 3 methods how activities can report their volume. 
            // #1: (and recommended) is to use an extension method that will remember on the STGDocument, STGPage or STGMedia that license volume was consumed:
            documentToProcess.ChargeVolume(GetLicenseId(), "volumeForAdjustedDocument", SigningFactory.CreateSigner(_secretHashKey));
            // #1.1: we do not need to use a signer, but if we don't use it, we cannot reliably verify that this license was charged by us. Any other activity can also do the following line:
            documentToProcess.ChargeVolume(GetLicenseId(), "volumeForAdjustedDocument_noSignature", null);
            // #1.2: we can of course also charge more than 1 license.
            documentToProcess.ChargeVolume(GetLicenseId(), "volumeForAdjustedDocument_multiCharge", SigningFactory.CreateSigner(_secretHashKey), 2);

            if (documentToProcess.HasChargedVolume(GetLicenseId(), "onlyChargedOnce", SigningFactory.CreateSigner(_secretHashKey)) == false)
            {
                // documentToProcess.HasChargedVolume returns true if at least 1 volume license was charged on this document for the volume name "onlyChargedOnce", and signed with out secret key.
                documentToProcess.ChargeVolume(GetLicenseId(), "onlyChargedOnce", SigningFactory.CreateSigner(_secretHashKey));
            }

            documentToProcess.HasChargedVolume(GetLicenseId(), "onlyChargedOnceEveryHour", SigningFactory.CreateSigner(_secretHashKey), out var paidLicenses);
            // If we are interested in when and how licenses were paid, we can ask for them. We still get true/false, but we could look into the list of paidLicenses and charge a second time if the last license charge was too long ago:
            if (paidLicenses.All(x => x.Data.Date < DateTime.UtcNow.AddHours(-1)) || paidLicenses.Any() == false)
            {
                // 5 days ago is too long ago, now we charge a second time. This adds another paidLicense entry to the list we'd get if we ask HasChargedVolume again
                documentToProcess.ChargeVolume(GetLicenseId(), "onlyChargedOnceEveryHour", SigningFactory.CreateSigner(_secretHashKey));
            }

            // NOTE: if we sign our charged volume, the check will require matching licenses to have a signature that can be confirmed.
            //       if we do not sign our volume, other activities can write the exact same paidLicenses entry as we do.
            // Charging for work items: We cannot store values on work items. So if you plan to charge volume per work item, we suggest you do this per root document,
            // as each root document is intrinsically linked to exactly one work item, and that pair is never going to be separated. The end-effect would be the same as
            // storing these values on a work item itself (if we could).

            // this function gets us all license entries that were charged on this document for our license ID.
            var all = documentToProcess.GetAllLicenseEntries(GetLicenseId());
            if (all.Any() && SigningFactory.CreateSigner(_secretHashKey).VerifyToken(all[0].Token))
            {
                // if we land here, we just verified ourselves that the all[0] license entry was created by us.
            }

            // we can also charge (and remember) volume on pages and media
            if (documentToProcess.Pages.Any())
            {
                documentToProcess.Pages[0].ChargeVolume(GetLicenseId(), "chargingOnPage", SigningFactory.CreateSigner(_secretHashKey));
            }
            if (documentToProcess.Media.Any())
            {
                documentToProcess.Media[0].ChargeVolume(GetLicenseId(), "chargingOnMedia", SigningFactory.CreateSigner(_secretHashKey));
            }

            // #2: we can also simply charge on the document, every time that we encounter the document. Using this method, we're not able to tell if we already charged a license on this document or not.
            // via the extension method AddUsedVolume(Guid, string, int), so that volume is only persisted by the platform when saving of a document succeeds.
            // All extension methods for the STGDocument and its elements are found in the STG.RT.API.Document.Licensing namespace
            documentToProcess.AddUsedVolume(GetLicenseId(), "totalExecutions", 1);

            // #3: Only in rare cases does it make sense to tell the platform about used volume without connecting it to a document.
            // For those rare cases, the STGProcess has a method for it:
            var volumeData = new DtoReportedActivityVolume();
            volumeData.LicenseId = GetLicenseId();
            volumeData.Data = new List<DtoActivityVolumeData>();
            volumeData.Data.Add(new DtoActivityVolumeData { Name = "totalExecutions2", Used = 1 });
            new STGProcess().ReportActivityVolumeAsync(volumeData).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Demonstrates how to check on used volume and how to compare it with the volume data that is signed and given by the activity vendor
        /// </summary>
        private void VerifyExistingVolume()
        {
            // the signed part of the activity license can tell you how much volume you gave the customer
            var volumeGivenInLicenseViaVendor = _licenseEntry.GetActivityLicense().Volumes;

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
            // let's check that the activity has not exceeded totalExecutions from what was given by the license.
            var mainVolume = _licenseEntry.Volumes?.FirstOrDefault(x => x.Name == "totalExecutions");
            if (mainVolume == null || mainVolume.Used >= mainVolume.MaxVolume + mainVolume.MaxVolume * (mainVolume.GraceVolume / 100.0))
            {
                Log.Debug("The LicensedActivitySample does not have volume for totalExecutions or has already exceeded the allowed amount.");
                throw new STG.Common.Utilities.Exceptions.STGLicenseException(STG.Common.Utilities.Exceptions.ErrorCode.LicenseMissingVolume, "The activity license volume 'totalExecutions' is exceeded");
            }

            // licenses can dynamically add volume counters to an activity license. That counter would not be on the _licenseEntry.LicenseToken, but you might want to give a user some test amount.
            var feature2Volume = _licenseEntry.Volumes?.FirstOrDefault(x => x.Name == "feature_2_volume");
            if (feature2Volume == null)
                return;
            var givenFeature2Volume = _license.Volumes.FirstOrDefault(x => x.Name == "feature_2_volume");
            if (feature2Volume.Used >= 5000 && givenFeature2Volume == null)
            {
                // if the volume for feature 2 was dynamically added (simply by calling _volumeReport.UsedVolume("feature_2_volume", 1) at least once), it will appear here
                // and we'll start warning after 5000 gratuitous executions.
                Log.Debug("The LicensedActivitySample got a feature_2_volume of 5000 units, but has already exceeded that.");
            }
            else
            {
                long maxWithGrace = 0;
                if (givenFeature2Volume != null)
                {
                    maxWithGrace = (long)(givenFeature2Volume.MaxVolume + givenFeature2Volume.MaxVolume * (givenFeature2Volume.GraceVolume / 100.0));
                }
                if (maxWithGrace <= feature2Volume.Used)
                {
                    // and in this case, the feature_2_volume was actually given in license of the activity vendor (you), but the activity has already used up more than the allotted amount.
                    Log.Debug("The LicensedActivitySample does not have volume for totalExecutions or has already exceeded the allowed amount.");
                    throw new STG.Common.Utilities.Exceptions.STGLicenseException(STG.Common.Utilities.Exceptions.ErrorCode.LicenseMissingVolume, "The activity license volume 'feature_2_volume' is exceeded");
                }
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
                _myLicense = null;
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
                _myLicense = null;
                return;
            }
            try
            {
                // the DtoActivityLicenseData.Payload contains the custom data that is transported via this license. features can be done with this.
                _myLicense = JsonConvert.DeserializeObject<MyLicense>(_license.LicensePayload);
            }
            catch (Exception ex)
            {
                _myLicense = null;
                Log.Debug("The activity license payload wasn't there", ex);
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
        /// Caleld by the activity host so that the activity license can be loaded and set.
        /// This guid is vendor generated and should be unique (at least to this vendor)
        /// </summary>
        public Guid GetLicenseId()
        {
            return Guid.Parse("2b93e963-87ab-484b-8eb3-9fc311bb2166");
        }

        private class MyLicense
        {
            public bool Feature1 { get; set; }
            public bool Feature2 { get; set; }
        }

        public class Config : ActivityConfigBase<Config>
        {
            public Config()
            {
                PlatformDataDesc = "This activity demonstrates how activity licensing can be used. If a license is given, it will write the custom value " +
                    "'Activity is processing' to the document. Depending on whether feature 1 or 2 are enabled via the custom part in the activity license, " +
                    "the custom values 'Feature 1' and 'Feature 2' are written. The written content will be the current date.";
            }

            [Display(Name = "Licensed Activity", Description = "Showcases how activities can be licensed")]
            [InputType(InputType.textarea), ReadOnly(true)]
            public string PlatformDataDesc { get; set; }
        }
    }
}
