using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleEventDrivenActivity.Configuration
{
    public class EventDrivenConfirmSettings : ActivityConfigBase<EventDrivenConfirmSettings>
    {

        public EventDrivenConfirmSettings()
        {
            DecisionCustomValue = "CV_Router";
        }

        [Display(Name = "Decision Custom Value Name", Description = "Defines a name of a custom value where the activity saves the decision.", Order = 1)]
        [InputType(InputType.text)]
        public string DecisionCustomValue { get; set; }

        [Display(Name = "Confirmation Message", Description = "Defines a confirmation message that is displayed to the user in the activity. If message is passed via query parameter, this value will override it.", Order = 1)]
        [InputType(InputType.text)]
        public string Message { get; set; }

        [Display(Name = "Positive Answer", Description = "Defines a button title and the value to be set in the custom value if user answers positive. If positive answer is passed via query parameter this value will override it.", Order = 2)]
        [InputType(InputType.text)]
        public string PositiveValue { get; set; }

        [Display(Name = "Negative Answer", Description = "Defines a button title and the value to be set in the custom value if user answers negative. If negative answer is passed via query parameter this value will override it.", Order = 3)]
        [InputType(InputType.text)]
        public string NegativeValue { get; set; }

        [Display(Name = "Activity URL", Description = "Placeholder for the variable containing the activity URL. This setting is actually used only for notifications, as must be known prior the activity is loaded", GroupName = "Location", Order = 4)]
        [InputType(InputType.url)]
        public string Location { get; set; }
    }
}
