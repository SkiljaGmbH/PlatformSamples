using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using SampleActivity.Properties;
using STG.Common.DTO.Configuration;
using STG.Common.DTO.Metadata;
using STG.Common.Interfaces.Activities;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class ConfigurationValidationSettings : ActivityConfigBase<ConfigurationValidationSettings>, IActivityConfigurationValidation
    {
        [Required]
        [Display(Name = nameof(Resources.ConfigurationValidationSettings_FilePath_Name), Description = nameof(Resources.ConfigurationValidationSettings_FilePath_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string FilePath { get; set; }

        [Display(Name = nameof(Resources.ConfigurationValidationSettings_IfThisIsTrue_Name), Description = nameof(Resources.ConfigurationValidationSettings_IfThisIsTrue_Description), GroupName = nameof(Resources.ConfigurationValidationSettings_IfThisIsTrue_GroupName), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        public bool IfThisIsTrue { get; set; }

        [Display(Name = nameof(Resources.ConfigurationValidationSettings_ThisMustNotBeEmpty_Name), Description = nameof(Resources.ConfigurationValidationSettings_ThisMustNotBeEmpty_Description), GroupName = nameof(Resources.ConfigurationValidationSettings_ThisMustNotBeEmpty_GroupName), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string ThisMustNotBeEmpty { get; set; }

        [Display(Name = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInThePast_Name), Description = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInThePast_Description), GroupName = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInThePast_GroupName), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.date)]
        public DateTime ThisMustBeInThePast { get; set; }

        [Display(Name = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInTheFuture_Name), Description = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInTheFuture_Description), GroupName = nameof(Resources.ConfigurationValidationSettings_ThisMustBeInTheFuture_GroupName), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.date)]
        public DateTime ThisMustBeInTheFuture { get; set; }

        public ConfigurationValidationSettings()
        {
            FilePath = "C:\\bla.txt";
            IfThisIsTrue = false;
        }

        public IList<DtoActivityConfigurationValidationResult> Validate(DtoActivityConfigurationValidationOptions options)
        {
            var ret = new List<DtoActivityConfigurationValidationResult>();

            if (File.Exists(FilePath) == false)
            {
                // info on DT, error on RT environment
                ret.Add(new DtoActivityConfigurationValidationResult()
                {
                    Level = options.IsDesigntimeEnvironment ? DtoActivityConfigurationValidationLevel.Info : DtoActivityConfigurationValidationLevel.Error,
                    PropertyName = nameof(FilePath),
                    Message = "The file specified in FilePath does not exist!"
                });
            }

            if (IfThisIsTrue &&
                string.IsNullOrWhiteSpace(ThisMustNotBeEmpty))
            {
                ret.Add(new DtoActivityConfigurationValidationResult()
                {
                    Level = DtoActivityConfigurationValidationLevel.Error,
                    PropertyName = nameof(ThisMustNotBeEmpty),
                    Message = "IfThisIsTrue is true, but ThisMustNotBeEmpty is empty!"
                });
            }

            if (ThisMustBeInThePast > DateTime.Today)
            {
                ret.Add(new DtoActivityConfigurationValidationResult()
                {
                    Level = DtoActivityConfigurationValidationLevel.Warning,
                    PropertyName = nameof(ThisMustBeInThePast),
                    Message = "ThisMustBeInThePast must be in the past!"
                });
            }

            if (ThisMustBeInTheFuture < DateTime.Today)
            {
                ret.Add(new DtoActivityConfigurationValidationResult()
                {
                    Level = DtoActivityConfigurationValidationLevel.Warning,
                    PropertyName = nameof(ThisMustBeInTheFuture),
                    Message = "ThisMustBeInTheFuture must be in the future!"
                });
            }

            return ret;
        }


    }
}
