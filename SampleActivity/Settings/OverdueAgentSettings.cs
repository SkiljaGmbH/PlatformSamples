using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SampleActivity.Properties;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class OverdueAgentSettings : ActivityConfigBase<OverdueAgentSettings>
    {

        public OverdueAgentSettings()
        {
            ReadMe = Resources.OverdueAgentSettings_Readme_Prompt;

            DaysOverdue = 7;
            IncludeCloseTo = false;
            ServerSettings = new SmtpSettings();


        }

        [Display(Name = nameof(Resources.OverdueAgentSettings_Readme_Name), Description = nameof(Resources.OverdueAgentSettings_Readme_Description), GroupName = nameof(Resources.OverdueAgentSettings_Readme_GroupName), Prompt = nameof(Resources.OverdueAgentSettings_Readme_Prompt),  Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea)]
        [Required]
        public string ReadMe { get; set; }

        [Display(Name = nameof(Resources.OverdueAgentSettings_DaysOverdue_Name), Description = nameof(Resources.OverdueAgentSettings_DaysOverdue_Description), GroupName = nameof(Resources.OverdueAgentSettings_DaysOverdue_GroupName), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, @"[{'Key':1,'Value':'One day or more'},{'Key':2,'Value':'Two Days or more'},{'Key':3,'Value':'Three days or more'},{'Key':5,'Value':'Five days or more'},{'Key':7,'Value':'A week or more'}]")]
        //[LocalizedValueProvider(KeyPattern = "OverdueAgentSettings_DaysOverdue_Values_{0}", ResourceType = typeof(Resources))]
        [Required]
        public int DaysOverdue { get; set; }

        [Display(Name = nameof(Resources.OverdueAgentSettings_IncludeCloseTo_Name), Description = nameof(Resources.OverdueAgentSettings_IncludeCloseTo_Description), GroupName = nameof(Resources.OverdueAgentSettings_IncludeCloseTo_GroupName), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        [Required]
        public bool IncludeCloseTo { get; set; }

        [Display(Name = nameof(Resources.OverdueAgentSettings_SendTo_Name), Description = nameof(Resources.OverdueAgentSettings_SendTo_Description), GroupName = nameof(Resources.OverdueAgentSettings_SendTo_GroupName), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string SendTo { get; set; }


        [Display(Name = nameof(Resources.OverdueAgentSettings_SendToMail_Name), Description = nameof(Resources.OverdueAgentSettings_SendToMail_Description), GroupName = nameof(Resources.OverdueAgentSettings_SendToMail_GroupName), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.email)]
        [Required]
        public string SendToMail { get; set; }

        [Display(Name = nameof(Resources.OverdueAgentSettings_ServerSettings_Name), Description = nameof(Resources.OverdueAgentSettings_ServerSettings_Description), GroupName = nameof(Resources.OverdueAgentSettings_ServerSettings_GroupName), Order = 6, ResourceType = typeof(Resources))]
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


        [Display(Name = nameof(Resources.SmtpSettings_ServerAddress_Name), Description = nameof(Resources.SmtpSettings_ServerAddress_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string ServerAddress { get; set; }

        [Display(Name = nameof(Resources.SmtpSettings_ServerPort_Name), Description = nameof(Resources.SmtpSettings_ServerPort_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        [Required]
        public int ServerPort { get; set; }

        [Display(Name = nameof(Resources.SmtpSettings_UseSSL_Name), Description = nameof(Resources.SmtpSettings_UseSSL_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        [Required]
        public bool UseSSL { get; set; }


        [Display(Name = nameof(Resources.SmtpSettings_UserName_Name), Description = nameof(Resources.SmtpSettings_UserName_Description), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string UserName { get; set; }

        [Display(Name = nameof(Resources.SmtpSettings_From_Name), Description = nameof(Resources.SmtpSettings_From_Description), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.email)]
        public string From { get; set; }

        [Display(Name = nameof(Resources.SmtpSettings_Password_Name), Description = nameof(Resources.SmtpSettings_Password_Description), Order = 6, ResourceType = typeof(Resources))]
        [InputType(InputType.password)]
        [Required]
        public string Password { get; set; }
    }
}

