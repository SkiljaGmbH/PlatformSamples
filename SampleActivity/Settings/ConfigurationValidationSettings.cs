using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using STG.Common.DTO.Configuration;
using STG.Common.DTO.Metadata;
using STG.Common.Interfaces.Activities;
using STG.RT.API.Activity;

namespace SampleActivity.Settings
{
    public class ConfigurationValidationSettings : ActivityConfigBase<ConfigurationValidationSettings>, IActivityConfigurationValidation
    {
        [Required]
        [Display(Name = "File Path", Description = "Just a path to a file", Order = 1)]
        [InputType(InputType.text)]
        public string FilePath { get; set; }

        [Display(Name = "If This Is True", Description = "If IfThisIsTrue is true, then ThisMustNotBeEmpty must not be empty", GroupName = "Just a Group", Order = 2)]
        [InputType(InputType.checkbox)]
        public bool IfThisIsTrue { get; set; }

        [Display(Name = "This Must Not Be Empty", Description = "If IfThisIsTrue is true, then ThisMustNotBeEmpty must not be empty", GroupName = "Just a Group", Order = 3)]
        [InputType(InputType.text)]
        public string ThisMustNotBeEmpty { get; set; }

        [Display(Name = "This Must Be In The Past", Description = "This Date Must Be In The Past", GroupName = "Just another Group", Order = 4)]
        [InputType(InputType.date)]
        public DateTime ThisMustBeInThePast { get; set; }

        [Display(Name = "This Must Be In The Future", Description = "This Date Must Be In The Future", GroupName = "Just another Group", Order = 5)]
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
