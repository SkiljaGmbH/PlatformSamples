using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ActivitySettings
{
    public class ActivityConfiguration : ActivityConfigBase<ActivityConfiguration>
    {
        private const string Dependent = "1. Dependant settings";
        private const string Binary = "2. Binary File";
        private const string Complex = "3. Database Lookups (Complex classes)";

        [Display(Name = "JSON File", Description = "JSON file sample", GroupName = Binary, Order = 1)]
        [InputType(InputType.file)]
        public DtoFileProperty JSONSettings { get; set; }

        [Display(Name = "JSON File Description", Description = "JSON file sample", GroupName = Binary, Order = 2)]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string JSONSettingsDesc { get; set; }

        [Display(Name = "Check box data", Description = "Check box sample", GroupName = Dependent, Order = 3)]
        [InputType(InputType.checkbox)]
        public bool CheckBoxData { get; set; }

        [Display(Name = "Check box dependent item", Description = "Item dependent on a check box", GroupName = Dependent, Order = 4)]
        [InputType(InputType.number)]
        public int CBDependentItem { get; set; }

        [Display(Name = "Check box dependent list", Description = "Item list dependent on a check box", GroupName = Dependent, Order = 5)]
        [InputType(InputType.listOfValues)]
        public List<string> LstDependent { get; set; }

        [Display(Name = "Dependency Description", Description = "JSON file sample", GroupName = Dependent, Order = 6)]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string CBDependencyDesc { get; set; }

        [Display(Name = "Database Lookup", GroupName = Complex, Description = "Database lookup settings", Order = 6)]
        [InputType(InputType.nestedClass)]
        public DatabaseLookup DatabaseLookup { get; set; }

        [Ignore]
        public List<DatabaseLookup> DatabaseLookups { get; set; }

        [Display(Name = "Database Lookup Description", Description = "JSON file sample", GroupName = Complex, Order = 7)]
        [InputType(InputType.textarea), ReadOnly(true)]
        public string DBLookupDesc { get; set; }

        [Display(Name = "Media Type selector", GroupName = "Platform data", Description = "Stores the one of available media types", Order = 1)]
        [InputType(InputType.text), ReadOnly(true)]
        public string MediaType { get; set; }

        [Display(Name = "Document Type selector", GroupName = "Platform data", Description = "Stores the one of document types assigned to the process", Order = 2)]
        [InputType(InputType.text), ReadOnly(true)]
        public string DocumentType { get; set; }

        [Display(Name = "Selected fields", GroupName = "Platform data", Description = "Stores the subset of the fields from the document type above", Order = 3)]
        [InputType(InputType.text), ReadOnly(true)]
        public string IndexFields { get; set; }

        [Display(Name = "Platform Data Description", Description = "Describes usage of the platform data", GroupName = "Platform data", Order = 4)]
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
