using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class CommonSettings : ActivityConfigBase<CommonSettings>
    {

        public CommonSettings()
        {
            RoutingCustomValueName = "ImportSource";
            FilteringCustomValueName = "ImportedAsJPG";
        }

        [Required]
        [Display(Name = nameof(Resources.CommonSettings_RoutingCustomValueName_Name), Description = nameof(Resources.CommonSettings_RoutingCustomValueName_Description), Order = 1, ResourceType = typeof(Resources))]
        public string  RoutingCustomValueName { get; set; }

        [Required]
        [Display(Name = nameof(Resources.CommonSettings_FilteringCustomValueName_Name), Description = nameof(Resources.CommonSettings_FilteringCustomValueName_Description), Order = 2, ResourceType = typeof(Resources))]
        public string FilteringCustomValueName { get; set; }
    }
}
