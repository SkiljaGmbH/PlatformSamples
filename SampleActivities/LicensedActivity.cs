using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
    public class LicensedActivity : STGUnattendedAbstract<LicensedActivity.Config>, ISTGLicensedActivity
    {
        private string _secureToken;
        private DtoActivityLicenseData _license;
        private MyLicense _myLicense;
        private ILicenseVerifier _verifier;

        /// <summary>
        /// ISTGLicensedActivity.SkipLicenseValidation
        /// With this mechanism, activity license verification is completely in the hands of the implementer.
        /// This can be used if activities should be able to run without a license - or to just limit certain features to just work with a license
        /// </summary>
        public bool SkipLicenseValidation => true;

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
            VerifyLicense();

            var customValueText = "executed at " + DateTime.UtcNow.ToString("u");
            documentToProcess.AddCustomValue("Activity is processing", customValueText, false);

            if (_myLicense.Feature1)
            {
                documentToProcess.AddCustomValue("Feature 1", customValueText, false);
            }

            // activity volume can be reported by telling the document about it via an extension method "AddUsedVolume"
            if (_myLicense.Feature2)
            {
                documentToProcess.AddCustomValue("Feature 2", customValueText, false);
                documentToProcess.AddUsedVolume(GetLicenseId(), "feature2", 1);
            }

            documentToProcess.AddUsedVolume(GetLicenseId(), "totalExecutions", 1);

            // if you want to tell the platform about used volume immediately, you can use the STGProcess:
            var stgProcess = new STGProcess();
            var volumeData = new DtoReportedActivityVolume();
            volumeData.LicenseId = GetLicenseId();
            volumeData.Data = new List<DtoActivityVolumeData>();
            volumeData.Data.Add(new DtoActivityVolumeData
            {
                Name = "immediatelyReported",
                Used = 1,
                Description = "my description for a dynamic volume",
                MaxVolume = 5000, // The MaxVolume value will be discarded by the platform
                GraceVolume = 1000 // the GraceVolume value will be discarded by the platform
            });

            stgProcess.ReportActivityVolumeAsync(volumeData).GetAwaiter().GetResult();
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

            if (licenseEntry.MergedLicenseTokens != null)
            {
                try
                {
                    string mergedLicenseData = string.Empty;
                    foreach (var token in licenseEntry.MergedLicenseTokens)
                    {
                        var isVerified = licenseVerifier.VerifySignature(token.Token, GetPublicKey());
                        var licenseData = token.Decrypt();
                        if (!isVerified) mergedLicenseData += "(Signature not verified) ";
                        mergedLicenseData += $"Name: {licenseData.Name}, Description: {licenseData.Description}, Issued: {licenseData.IssuedDate}\r\n";
                    }
                    Log.Debug("The activity license has been merged with the use of the following licenses:\r\n" + mergedLicenseData);
                }
                catch (Exception ex)
                {
                    Log.Debug("MergedLicenseTokens could not be read", ex);
                }
            }
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
