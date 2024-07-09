using System;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Import Path", Description = "Import path.", Order = 1)]
        public string ImportPath { get; set; }

        [Required]
        [Display(Name = "Jpeg Filetr", Description = "Jpeg filetr.", Order = 2)]
        public string JpegFilter { get; set; }

        [Required]
        [Display(Name = "Tiff Filetr", Description = "Tiff filetr.", Order = 3)]
        public string TiffFilter { get; set; }

        [Display(Name = "Document Type", Description = "", Order = 4)]
        public string DocumentType { get; set; }

        [InputType(InputType.nestedClass)]
        [Display(Name = "Common Settings", Description = "Common settings.", Order = 5)]
        public CommonSettings Common { get; set; }
    }
}
