using System.ComponentModel.DataAnnotations;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class OverdueAgentSettings : ActivityConfigBase<OverdueAgentSettings>
    {

        public OverdueAgentSettings()
        {
            ReadMe = "This system agent ticks as configured on the process settings tab. " +
                "With every tick it counts the number of work items that have the SLA status in Overdue " +
                "(and alternatively in Close To, depending on the configuration) and that they are in this status for longer than configured number of days." +
                " If there are such work items the agent will compose a mail like this:  " +
                "'Dear <Send To>. We have detected that the process <name of the process> have <number of work items> work items in Overdue for more than <Days Overdue>. Kind regards." +
                "This e-mail will be sent on the e-mail configured in Send To Address property using SMTP server as configured in SMTP Server Settings property." +
                "Please make sure you do not tick too often, or this agent will spam the receiver.";

            DaysOverdue = 7;
            IncludeCloseTo = false;
            ServerSettings = new SmtpSettings();


        }

        [Display(Name = "ReadMe", Description = "Explains what this system agent does.", GroupName = "01 - Description", Order = 1)]
        [InputType(InputType.textarea)]
        [Required]
        public string ReadMe { get; set; }

        [Display(Name = "Days Overdue", Description = "Number of days the work item must be in the overdue SLA status in order to be counted for notification.", GroupName = "02 - Notification", Order = 2)]
        [InputType(InputType.enumeration, @"[{'Key':1,'Value':'One day or more'},{'Key':2,'Value':'Two Days or more'},{'Key':3,'Value':'Three days or more'},{'Key':5,'Value':'Five days or more'},{'Key':7,'Value':'A week or more'}]")]
        [Required]
        public int DaysOverdue { get; set; }

        [Display(Name = "Including CloseTo", Description = "If this option is selected, the agent will count also work items that are in the CloseTo status.", GroupName = "02 - Notification", Order = 3)]
        [InputType(InputType.checkbox)]
        [Required]
        public bool IncludeCloseTo { get; set; }

        [Display(Name = "Send To", Description = "The name of the receiver.", GroupName = "03 - E-Mail Settings", Order = 4)]
        [InputType(InputType.text)]
        [Required]
        public string SendTo { get; set; }


        [Display(Name = "Send To Address", Description = "The E-mail address where the notification mail will be sent to.", GroupName = "03 - E-Mail Settings", Order = 5)]
        [InputType(InputType.email)]
        [Required]
        public string SendToMail { get; set; }

        [Display(Name = "SMTP Server Settings", Description = "Configuration of the SMTP server to be used for sending notification e-mails.", GroupName = "03 - E-Mail Settings", Order = 6)]
        [InputType(InputType.nestedClass)]
        [Required]
        public SmtpSettings ServerSettings { get; set; }

    }

    public class SmtpSettings : ActivityConfigBase<SmtpSettings>
    {
        public SmtpSettings()
        {
            ServerPort = 25;
        }


        [Display(Name = "Server Address", Description = "The address of the SMTP server.", Order = 1)]
        [InputType(InputType.text)]
        [Required]
        public string ServerAddress { get; set; }

        [Display(Name = "Server Port", Description = "The port of the SMTP server.", Order = 2)]
        [InputType(InputType.number)]
        [Required]
        public int ServerPort { get; set; }

        [Display(Name = "Use SSL", Description = "Use SSL for establishing connection.", Order = 3)]
        [InputType(InputType.checkbox)]
        [Required]
        public bool UseSSL { get; set; }


        [Display(Name = "User Name", Description = " The user for authorizing on SMTP server.", Order = 4)]
        [InputType(InputType.text)]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "from e-mail address", Description = "The e mail address of the sender. If not set the UserName will be used.", Order = 5)]
        [InputType(InputType.email)]
        public string From { get; set; }

        [Display(Name = "Password", Description = "The password for the user to authorize on SMTP server.", Order = 6)]
        [InputType(InputType.password)]
        [Required]
        public string Password { get; set; }
    }
}

