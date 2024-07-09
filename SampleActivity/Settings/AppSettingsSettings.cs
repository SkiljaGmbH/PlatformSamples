using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class AppSettingsSettings : ActivityConfigBase<AppSettingsSettings>
    {

        [Required]
        [Display(Name = "AppSettings", Description = "Comma separated list of AppSettings to output.", Order = 1)]
        public string AppSettingsToOutput { get; set; }

        [Required]
        [Display(Name = "Output Custom Value", Description = "Custom value where to output AppSettings.", Order = 2)]
        public string OutputCustomValue { get; set; }
    }
}
