using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
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

        [Display(Name = nameof(Resources.WatermarkSettings_Common_Name), Order = 4, ResourceType = typeof(Resources)), InputType(InputType.nestedClass)]
        public CommonSettings Common { get; set; }

        [Required]
        [Display(Name = nameof(Resources.WatermarkSettings_WatermarkText_Name), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string WatermarkText { get; set; }
    }
}
