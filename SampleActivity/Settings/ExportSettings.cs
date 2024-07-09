using System;
using System.ComponentModel.DataAnnotations;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class ExportSettings : ActivityConfigBase<ExportSettings>
    {
        [Display(Name = "Routing and Filtering Settings", Order = 4), InputType(InputType.nestedClass)]
        public CommonSettings Common { get; set; }

        [Required]
        [Display(Name = "Export Path", Description = "Path to where the files are going to be exported", Order = 1)]
        [InputType(InputType.text)]
        public string ExportPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Media Mapping", Description = "Maps media to be exported to the extensions of exported files", Order = 2)]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> ExportMediaMappings { get; set; }

        [Display(Name = "Flat Export", Description = "All the files are put in the same (export) folder", Order = 3)]
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
