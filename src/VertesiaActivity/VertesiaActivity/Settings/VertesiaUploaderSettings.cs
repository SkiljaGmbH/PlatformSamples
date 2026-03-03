using System.ComponentModel.DataAnnotations;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using VertesiaActivity.Properties;

namespace VertesiaActivity.Settings
{
    public class VertesiaUploaderSettings : ActivityConfigBase<VertesiaUploaderSettings>
    {
        public VertesiaUploaderSettings()
        {
            ResultMapping = new SerializableDictionary<string, string>();
        }

        /// <summary>
        /// API key passed in the Authorization header to the Vertesia STS endpoint
        /// in exchange for a JWT bearer token.
        /// </summary>
        [Required]
        [Display(
            Name = nameof(Resources.VertesiaUploaderSettings_ApiKey_Name),
            Description = nameof(Resources.VertesiaUploaderSettings_ApiKey_Description),
            Order = 1,
            ResourceType = typeof(Resources))]
        [InputType(InputType.password)]
        public string ApiKey { get; set; }

        /// <summary>
        /// Base URL of the Vertesia API (cv_API_URL).
        /// All API calls are constructed relative to this value.
        /// </summary>
        [Required]
        [Display(
            Name = nameof(Resources.VertesiaUploaderSettings_ApiUrl_Name),
            Description = nameof(Resources.VertesiaUploaderSettings_ApiUrl_Description),
            Order = 2,
            ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string ApiUrl { get; set; }

        /// <summary>
        /// Content type used when requesting an upload URL and registering
        /// the uploaded object (cv_content_type).
        /// </summary>
        [Required]
        [Display(
            Name = nameof(Resources.VertesiaUploaderSettings_ContentType_Name),
            Description = nameof(Resources.VertesiaUploaderSettings_ContentType_Description),
            Order = 3,
            ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string ContentType { get; set; }

        /// <summary>
        /// ID of the Vertesia interaction to execute after the object reaches
        /// "ready" status (cv_interaction_id).
        /// </summary>
        [Required]
        [Display(
            Name = nameof(Resources.VertesiaUploaderSettings_InteractionId_Name),
            Description = nameof(Resources.VertesiaUploaderSettings_InteractionId_Description),
            Order = 4,
            ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string InteractionId { get; set; }

        /// <summary>
        /// Maps fields from the interaction Results JSON to custom value names
        /// on the child document.
        /// Key   = field name in the Results object returned by the interaction.
        /// Value = custom value name to set on the child document.
        /// </summary>
        [Display(
            Name = nameof(Resources.VertesiaUploaderSettings_ResultMapping_Name),
            Description = nameof(Resources.VertesiaUploaderSettings_ResultMapping_Description),
            Order = 5,
            ResourceType = typeof(Resources))]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> ResultMapping { get; set; }
    }
}
