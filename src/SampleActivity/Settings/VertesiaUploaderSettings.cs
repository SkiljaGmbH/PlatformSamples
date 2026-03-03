using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.Common.DTO;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class VertesiaUploaderSettings : ActivityConfigBase<VertesiaUploaderSettings>
    {
        public VertesiaUploaderSettings()
        {
            ResultMapping = new SerializableDictionary<string, string>();
        }

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

        [Display(Name = nameof(Resources.VertesiaUploaderSettings_InteractionId_Name), Description = nameof(Resources.VertesiaUploaderSettings_InteractionId_Description), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string InteractionId { get; set; }

        [Display(Name = nameof(Resources.VertesiaUploaderSettings_ResultMapping_Name), Description = nameof(Resources.VertesiaUploaderSettings_ResultMapping_Description), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> ResultMapping { get; set; }
    }
}
