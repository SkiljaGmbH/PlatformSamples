using System.ComponentModel.DataAnnotations;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class WatermarkSettings : ActivityConfigBase<WatermarkSettings>
    {
        public WatermarkSettings()
        {
            WatermarkText = "Giulia Demo";
            Common = new CommonSettings();
        }

        [Display(Name = "Routing and Filtering Settings", Order = 4), InputType(InputType.nestedClass)]
        public CommonSettings Common { get; set; }

        [Required]
        [Display(Name = "Watermark Text", Description = "", Order = 1)]
        [InputType(InputType.text)]
        public string WatermarkText { get; set; }
    }
}
