using SampleActivity.Properties;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using STG.Common.Utilities.Activities;

namespace SampleActivity.Configuration
{
    /// <summary>
    /// This activity demonstrates how to localize values in activity settings and how they respect (or do not respect) UI language switching.
    /// </summary>
    public class Localizer : ActivityConfigBase<Localizer>
    {
        /// <summary>
        /// The proper way to set a localized value for a read-only text field.
        /// When the value is set in the <see cref="DisplayAttribute.Prompt"/> attribute, the activity does not store the value.
        /// Instead, the UI generates it based on the value from the Prompt attribute.
        /// This approach ensures that the value respects the UI culture and updates dynamically when the UI language switches.
        /// </summary>
        [Display(Name = nameof(Resources.Localizer_DescFromPrompt_Name), Description = nameof(Resources.Localizer_DescFromPrompt_Description), Prompt = nameof(Resources.Localizer_DescFromPrompt_Prompt), ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DescFromPrompt { get; set; }

        /// <summary>
        /// A common mistake: overriding a value set in the <see cref="DisplayAttribute.Prompt"/> attribute within the constructor (or activity initialization method).
        /// When overridden, the constructor-assigned value takes precedence over the Prompt attribute.
        /// Although the value is still taken from resources, it respects the UI culture only at the moment of activity registration.
        /// The overridden value is stored as a constant in the settings and does not change when the UI language switches.
        /// To reset it to the correct localized value, one must manually reset the default activity settings in the editor.
        /// Resetting activity settings restores the value based on the UI culture at the time of the reset.
        /// </summary>
        [Display(Name = nameof(Resources.Localizer_PromptOverride_Name), Description = nameof(Resources.Localizer_PromptOverride_Description), Prompt = nameof(Resources.Localizer_PromptOverride_Prompt), ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string PromptOverride { get; set; }

        /// <summary>
        /// Read-only values can be defined without using the Prompt attribute.
        /// Instead, their value can be set in the constructor, as demonstrated here.
        /// The constructor-assigned value is still retrieved from resources and respects the UI culture only at the moment of activity registration.
        /// However, it is then stored in the settings as a constant.
        /// Once stored, the value does not change when the UI language switches.
        /// To update it, one must manually reset the default activity settings in the editor.
        /// Resetting activity settings restores the property value based on the UI culture at the time of the reset.
        /// </summary>
        [Display(Name = nameof(Resources.Localizer_DescFromCtor_Name), Description = nameof(Resources.Localizer_DescFromCtor_Description), ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DescFromCtor { get; set; }

        /// <summary>
        /// Similar to <see cref="DescFromCtor"/>, but assigns the default value inline.
        /// The value is still retrieved from resources and respects the UI culture only at the moment of activity registration.
        /// However, it is stored as a constant in the settings and does not change when the UI language switches.
        /// To update it, one must manually reset the default activity settings in the editor.
        /// Resetting activity settings restores the property value based on the UI culture at the time of the reset.
        /// </summary>
        [Display(Name = nameof(Resources.Localizer_DescFromInLine_Name), Description = nameof(Resources.Localizer_DescFromInLine_Description), ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DescFromInLine { get; set; } = Resources.Localizer_DescFromInLine_Prompt;

        /// <summary>
        /// Default values for properties can also be set in the override of the <see cref="ActivityConfigLoader{TActivityConfig}.Initialize(System.IO.Stream)"/> method of the activity.
        /// This allows developers to define different default values based on configured activity settings.
        /// Like <see cref="DescFromInLine"/> and <see cref="DescFromCtor"/>, the value is retrieved from resources and respects the UI culture only at the moment of activity registration.
        /// Once set, the value is stored as a constant in the settings and does not change when the UI language switches.
        /// </summary>
        [Display(Name = nameof(Resources.Localizer_DescFromInit_Name), Description = nameof(Resources.Localizer_DescFromInit_Description), ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DescFromInit { get; set; }

        public Localizer()
        {
            DescFromCtor = Resources.Localizer_DescFromCtor_Prompt;
            PromptOverride = Resources.Localizer_PromptOverride_PromptOverride;
        }
    }

}
