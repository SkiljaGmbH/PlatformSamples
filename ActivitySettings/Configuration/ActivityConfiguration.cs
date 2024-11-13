using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ActivitySettings.Properties;

namespace ActivitySettings.Configuration
{
    public class ActivityConfiguration : ActivityConfigBase<ActivityConfiguration>
    {
        private const string Dependent = "1. Dependant settings";
        private const string Binary = "2. Binary File";
        private const string Complex = "3. Database Lookups (Complex classes)";
        [Display(Name = nameof(Resources.ActivityConfiguration_JSONSettings_Name), Description = nameof(Resources.ActivityConfiguration_JSONSettings_Description), GroupName = nameof(Resources.ActivityConfiguration_JSONSettings_GroupName), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.file)]
        public DtoFileProperty JSONSettings { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_JSONSettingsDesc_Name), Description = nameof(Resources.ActivityConfiguration_JSONSettingsDesc_Description), GroupName = nameof(Resources.ActivityConfiguration_JSONSettingsDesc_GroupName), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string JSONSettingsDesc { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_CheckBoxData_Name), Description = nameof(Resources.ActivityConfiguration_CheckBoxData_Description), GroupName = nameof(Resources.ActivityConfiguration_CheckBoxData_GroupName), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        public bool CheckBoxData { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_CBDependentItem_Name), Description = nameof(Resources.ActivityConfiguration_CBDependentItem_Description), GroupName = nameof(Resources.ActivityConfiguration_CBDependentItem_GroupName), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        public int CBDependentItem { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_LstDependent_Name), Description = nameof(Resources.ActivityConfiguration_LstDependent_Description), GroupName = nameof(Resources.ActivityConfiguration_LstDependent_GroupName), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.listOfValues)]
        public List<string> LstDependent { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_CBDependencyDesc_Name), Description = nameof(Resources.ActivityConfiguration_CBDependencyDesc_Description), GroupName = nameof(Resources.ActivityConfiguration_CBDependencyDesc_GroupName), Order = 6, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string CBDependencyDesc { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_DatabaseLookup_Name), GroupName = nameof(Resources.ActivityConfiguration_DatabaseLookup_GroupName), Description = nameof(Resources.ActivityConfiguration_DatabaseLookup_Description), Order = 6, ResourceType = typeof(Resources))]
        [InputType(InputType.nestedClass)]
        public DatabaseLookup DatabaseLookup { get; set; }

        [Ignore]
        public List<DatabaseLookup> DatabaseLookups { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_DBLookupDesc_Name), Description = nameof(Resources.ActivityConfiguration_DBLookupDesc_Description), GroupName = nameof(Resources.ActivityConfiguration_DBLookupDesc_GroupName), Order = 7, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DBLookupDesc { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_MediaType_Name), GroupName = nameof(Resources.ActivityConfiguration_MediaType_GroupName), Description = nameof(Resources.ActivityConfiguration_MediaType_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text), ReadOnly(true)]
        public string MediaType { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_DocumentType_Name), GroupName = nameof(Resources.ActivityConfiguration_DocumentType_GroupName), Description = nameof(Resources.ActivityConfiguration_DocumentType_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.text), ReadOnly(true)]
        public string DocumentType { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_IndexFields_Name), GroupName = nameof(Resources.ActivityConfiguration_IndexFields_GroupName), Description = nameof(Resources.ActivityConfiguration_IndexFields_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.text), ReadOnly(true)]
        public string IndexFields { get; set; }

        [Display(Name = nameof(Resources.ActivityConfiguration_PlatformDataDesc_Name), Description = nameof(Resources.ActivityConfiguration_PlatformDataDesc_Description), GroupName = nameof(Resources.ActivityConfiguration_PlatformDataDesc_GroupName), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string PlatformDataDesc { get; set; }

        public ActivityConfiguration()
        {
            JSONSettings = new DtoFileProperty();
            JSONSettingsDesc = "On the advanced settings you can edit the uploaded JSON. or create one from scratch";
            CheckBoxData = true;
            CBDependentItem = 30;
            CBDependencyDesc = "On the advanced settings you can make the Check box dependent item disabled depending if the check box is checked.";
            DatabaseLookup = new DatabaseLookup();
            DatabaseLookups = new List<DatabaseLookup>();
            DBLookupDesc = "The activity allows setting up more than one DB lookup. Please open the advanced settings to see how this is done.";
            PlatformDataDesc = "Open the advanced settings to select the media type, document type and the fields from the selected document type.";
            LstDependent = new List<string>();
        }
    }
}