using System;
using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class ExportSettings : ActivityConfigBase<ExportSettings>
    {
        [Display(Name = nameof(Resources.ExportSettings_Common_Name), Order = 4, ResourceType =typeof(Resources)), InputType(InputType.nestedClass)]
        public CommonSettings Common { get; set; }

        [Required]
        [Display(Name = nameof(Resources.ExportSettings_ExportPath_Name), Description = nameof(Resources.ExportSettings_ExportPath_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string ExportPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = nameof(Resources.ExportSettings_ExportMediaMappings_Name), Description = nameof(Resources.ExportSettings_ExportMediaMappings_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> ExportMediaMappings { get; set; }

        [Display(Name = nameof(Resources.ExportSettings_FlatExport_Name), Description = nameof(Resources.ExportSettings_FlatExport_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        public Boolean FlatExport { get; set; }

        public ExportSettings()
        {
            Common = new CommonSettings();
            ExportPath = @"..\Samples\SampleImages\Export";
            ExportMediaMappings = new SerializableDictionary<string, string>();
            ExportMediaMappings.Add("PDF", ".pdf");
            ExportMediaMappings.Add("Tiff", ".tif");
            ExportMediaMappings.Add("Jpg", ".jpg");
            ExportMediaMappings.Add("Text", ".ocr");
            FlatExport = false;
        }
    }
}
