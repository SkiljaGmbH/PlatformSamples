using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System.ComponentModel.DataAnnotations;

namespace SampleEventDrivenActivity.Configuration
{
    public class EventDrivenInitializerSettings : ActivityConfigBase<EventDrivenInitializerSettings>
    {
        public EventDrivenInitializerSettings()
        {
            ClassificationSettings = new ClassificationServiceData();
        }

        [Display(Name = "Root Document Name", Description = "Allows to set the root document's name.", Order = 2)]
        [InputType(InputType.text)]
        public string RootDocumentName { get; set; }

        [Display(Name = "Unknown Extensions",
            Description =
                "Defines how media with unknown extensions will be handled. They can either be ignored or imported as unknown.",
            Order = 5)]
        [InputType(InputType.enumeration, typeof(UnknownMediaExtensionHandling))]
        public UnknownMediaExtensionHandling HandleUnknownExtensions { get; set; }

        [Display(Name = "Create pages for medias", Description = "If set, a page will be created for each media.", Order = 7)]
        [InputType(InputType.checkbox)]
        public bool CreatePages { get; set; }

        [Display(Name = "Prefill Custom Values", Description = "Comma delimited list of custom value names to be prefilled via meta-data.", Order = 10)]
        [InputType(InputType.text)]
        public string PrefillCustomValues { get; set; }

        [Display(Name = "Short Description", Description = "Short description to explain what the process is doing.", Order = 11)]
        [InputType(InputType.text)]
        public string ShortDescription { get; set; }

        [Display(Name = "Long Description", Description = "Long description providing detailed explanation on how to handle the process from outside.", Order = 12)]
        [InputType(InputType.textarea)]
        public string LongDescription { get; set; }

        [Display(Name = "Classification Activity Interaction", Description = "Use this settings to request data required by classification activity like classification type and classification project", Order = 20)]
        [InputType(InputType.nestedClass)]
        public ClassificationServiceData ClassificationSettings { get; set; }
    }

    public enum UnknownMediaExtensionHandling
    {
        ImportAsUnknown = 0,
        Ignore = 1
    }
}
