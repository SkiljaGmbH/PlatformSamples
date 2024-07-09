using System.ComponentModel.DataAnnotations;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace SampleEventDrivenActivity.Configuration
{
    public class EventDrivenNotifierSettings : ActivityConfigBase<EventDrivenNotifierSettings>
    {
        [Display(Name = "Export fields and tables", Description = "If set, all fields and tables will be packaged.", Order = 1)]
        [InputType(InputType.checkbox)]
        public bool ExportFieldsAndTables { get; set; }

        [Display(Name = "Export all media", Description = "If set, all media will be in the resulting package.", Order = 2)]
        [InputType(InputType.checkbox)]
        public bool ExportMedia { get; set; }

        [Display(Name = "Export extensions", Description = "If set, all available extensions on document level are inserted into the resulting package.", Order = 3)]
        [InputType(InputType.checkbox)]
        public bool ExportExtensions { get; set; }
    }
}