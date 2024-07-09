using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Routing Custom Value Name", Description = "Routing custom value name.", Order = 1)]
        public string  RoutingCustomValueName { get; set; }

        [Required]
        [Display(Name = "Filtering Custom Value Name", Description = "Filtering custom value name.", Order = 2)]
        public string FilteringCustomValueName { get; set; }
    }
}
