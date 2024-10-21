using System;
using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class ImportSettings : ActivityConfigBase<ImportSettings>
    {
        public ImportSettings()
        {
            ImportPath = @"..\Samples\SampleImages";
            JpegFilter = "*.jpg";
            TiffFilter = "*.tif";
            DocumentType = String.Empty;
            Common = new CommonSettings();
        }

        [Required]
        [Display(Name = nameof(Resources.ImportSettings_ImportPath_Name), Description = nameof(Resources.ImportSettings_ImportPath_Description), Order = 1, ResourceType = typeof(Resources))]
        public string ImportPath { get; set; }

        [Required]
        [Display(Name = nameof(Resources.ImportSettings_JpegFilter_Name), Description = nameof(Resources.ImportSettings_JpegFilter_Description), Order = 2, ResourceType = typeof(Resources))]
        public string JpegFilter { get; set; }

        [Required]
        [Display(Name = nameof(Resources.ImportSettings_TiffFilter_Name), Description = nameof(Resources.ImportSettings_TiffFilter_Description), Order = 3, ResourceType = typeof(Resources))]
        public string TiffFilter { get; set; }

        [Display(Name = nameof(Resources.ImportSettings_DocumentType_Name), Description = nameof(Resources.ImportSettings_DocumentType_Description), Order = 4, ResourceType = typeof(Resources))]
        public string DocumentType { get; set; }

        [InputType(InputType.nestedClass)]
        [Display(Name = nameof(Resources.ImportSettings_Common_Name), Description = nameof(Resources.ImportSettings_Common_Description), Order = 5, ResourceType = typeof(Resources))]
        public CommonSettings Common { get; set; }
    }
}
