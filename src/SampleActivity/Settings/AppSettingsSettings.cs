using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleActivity.Properties;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class AppSettingsSettings : ActivityConfigBase<AppSettingsSettings>
    {

        [Required]
        [Display(Name = nameof(Resources.AppSettingsSettings_AppSettingsToOutput_Name), Description = nameof(Resources.AppSettingsSettings_AppSettingsToOutput_Description), Order = 1, ResourceType = typeof(Resources))]
        public string AppSettingsToOutput { get; set; }

        [Required]
        [Display(Name = nameof(Resources.AppSettingsSettings_OutputCustomValue_Name), Description = nameof(Resources.AppSettingsSettings_OutputCustomValue_Description), Order = 2, ResourceType = typeof(Resources))]
        public string OutputCustomValue { get; set; }
    }
}
