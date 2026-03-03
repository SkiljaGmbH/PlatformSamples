using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class VertesiaUploaderSettings : ActivityConfigBase<VertesiaUploaderSettings>
    {
        [Display(Name = nameof(Resources.VertesiaUploaderSettings_ApiKey_Name), Description = nameof(Resources.VertesiaUploaderSettings_ApiKey_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.password)]
        [Required]
        public string ApiKey { get; set; }

        [Display(Name = nameof(Resources.VertesiaUploaderSettings_ApiUrl_Name), Description = nameof(Resources.VertesiaUploaderSettings_ApiUrl_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string ApiUrl { get; set; }

        [Display(Name = nameof(Resources.VertesiaUploaderSettings_ContentType_Name), Description = nameof(Resources.VertesiaUploaderSettings_ContentType_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string ContentType { get; set; }
    }
}
