using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class SampleWithHeartbeatSettings : ActivityConfigBase<SampleWithHeartbeatSettings>
    {
        public const string DefaultHeartbeatMessage = "Still processing";

        [Display(Name = nameof(Resources.SampleWithHeartbeatSettings_ProcessingDurationSeconds_Name), Description = nameof(Resources.SampleWithHeartbeatSettings_ProcessingDurationSeconds_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        [Required]
        public int ProcessingDurationSeconds { get; set; } = 180;

        [Display(Name = nameof(Resources.SampleWithHeartbeatSettings_HeartbeatIntervalSeconds_Name), Description = nameof(Resources.SampleWithHeartbeatSettings_HeartbeatIntervalSeconds_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        [Required]
        public int HeartbeatIntervalSeconds { get; set; } = 20;

        [Display(Name = nameof(Resources.SampleWithHeartbeatSettings_HeartbeatMessage_Name), Description = nameof(Resources.SampleWithHeartbeatSettings_HeartbeatMessage_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string HeartbeatMessage { get; set; } = DefaultHeartbeatMessage;

        [Display(Name = nameof(Resources.SampleWithHeartbeatSettings_Common_Name), Order = 4, ResourceType = typeof(Resources)), InputType(InputType.nestedClass)]
        public CommonSettings Common { get; set; } = new CommonSettings();
    }
}
