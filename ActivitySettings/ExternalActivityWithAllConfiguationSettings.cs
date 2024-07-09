using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;


namespace ActivitySettings
{
    /// <summary>
    /// External activity with the ExternalSettings's properties
    /// </summary>
    public class ExternalActivityWithAllConfiguationSettings : STGExternalAbstract<ExternalSettings> { }
    
    /// <summary>
    /// Sample enumeration for drop down demonstration
    /// </summary>
    public enum EnumToDemo
    {
        EnumeratorValue1 = 0,
        EnumeratorValue2 = 1,
        EnumeratorValue3 = 2,
        EnumeratorValue4 = 3,
    }

    /// <summary>
    /// Sample settings
    /// </summary>
    public class ExternalSettings : ActivityConfigBase<ExternalSettings>
    {
        /// <summary>
        /// Sample string property rendered with one line text box
        /// </summary>
        [Display(Name = "String Property", Description = "Property of type string", GroupName = "Simple properties", Order = 1)]
        [InputType(InputType.text)]
        public string StringProperty { get; set; }

        /// <summary>
        /// Sample password property 
        /// </summary>
        [Display(Name = "Password Property", Description = "Property of type string with password characters", GroupName = "Simple properties", Order = 2)]
        [InputType(InputType.password)]
        public string PwdProperty { get; set; }

        /// <summary>
        /// Sample string property rendered with multi line text area
        /// </summary>
        [Display(Name = "String Property (long)", Description = "Property of type string with area input", GroupName = "Simple properties", Order = 3)]
        [InputType(InputType.textarea)]
        public string LongStringProperty { get; set; }

        /// <summary>
        /// Sample double property 
        /// </summary>
        [Display(Name = "Double Property", Description = "Property of type double", GroupName = "Simple properties", Order = 4)]
        [InputType(InputType.number)]
        public double DoubleProperty { get; set; }

        /// <summary>
        /// Sample boolean property rendered with check box
        /// </summary>
        [Display(Name = "Boolean Property", Description = "Property of type boolean", GroupName = "Simple properties", Order = 5)]
        [InputType(InputType.checkbox)]
        public bool BooleanProperty { get; set; }

        /// <summary>
        /// Sample date property rendered with date picker
        /// </summary>
        [Display(Name = "DateTime Property", Description = "Property of type date time", GroupName = "Simple properties", Order = 6)]
        [InputType(InputType.date)]
        public DateTime DateTimeProperty { get; set; }

        /// <summary>
        /// Sample color property rendered with color picker
        /// </summary>
        [Display(Name = "Color Property", Description = "Property of type color", GroupName = "Simple properties", Order = 7)]
        [InputType(InputType.colorpicker)]
        public string ColorProperty { get; set; }


        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with enumeration values
        /// </summary>
        [Display(Name = "Combo With Enumerator", Description = "Combo box for enumeration type property", GroupName = "Complex properties", Order = 8)]
        [InputType(InputType.enumeration, typeof(EnumToDemo))]
        public EnumToDemo EnumPropertyCombo { get; set; }

        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with provided key-value pair values
        /// </summary>
        [Display(Name = "Combo With dictionary", Description = "Combo box for integer dictionary property", GroupName = "Complex properties", Order = 9)]
        [InputType(InputType.enumeration, @"[{'Key':1,'Value':'One'},{'Key':2,'Value':'Two'},{'Key':3,'Value':'Three'}]")]
        public int IntegerCombo { get; set; }

        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with provided string values
        /// </summary>
        [Display(Name = "Combo With comma delimited values", Description = "Combo box for comma delimited strings property", GroupName = "Complex properties", Order = 10)]
        [InputType(InputType.enumeration, @"Option A;Option B;Option C")]
        public string CommaDelimitedCombo { get; set; }

        /// <summary>
        /// Sample property showing how to use the document type combo selection
        /// </summary>
        [Display(Name = "Available document types", Description = "Drop-down with document types assigned to the process", GroupName = "Complex properties", Order = 11)]
        [InputType(InputType.enumeration, PlatformDropdownType.document)]
        public string DocumentTypeSelection { get; set; }


        /// <summary>
        /// Sample property showing how to use the media type combo selection
        /// </summary>
        [Display(Name = "Available media types", Description = "Drop down with all media types available in the platform", GroupName = "Complex properties", Order = 12)]
        [InputType(InputType.enumeration, PlatformDropdownType.media)]
        public string MediaTypeSelection { get; set; }

        /// <summary>
        /// Sample string property, containing only the base64 encoded file stream, rendered with file uploader.
        /// </summary>
        /// <remarks>
        /// Although this sample allows variable on this field, this setting will be ignored when UI gets rendered.
        /// The reason is that variables on files can be used only if type of the property is DtoFileProperty
        /// </remarks>
        [Display(Name = "File Upload", Description = "File uploader for string property", GroupName = "Complex properties", Order = 13)]
        [InputType(InputType.file)]
        public string FileUploadProperty { get; set; }

        /// <summary>
        /// Sample dictionary property rendered with key-value pair grid
        /// </summary>
        [Display(Name = "Dummy Grid", Description = "Grid for string dictionary property", GroupName = "Complex properties", Order = 14)]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> GridProperty { get; set; }


        /// <summary>
        /// Sample custom nested class property rendered it its own group with the class's properties
        /// </summary>
        [Display(Name = "Nested Class", Description = "Nested class with file uploader as a property", Order = 15)]
        [InputType(InputType.nestedClass)]
        public NestedWithFiles NestedClassProperty { get; set; }

        [Display(Name = "Multi Field Selector", Description = "Showcase that demonstrates how to select fields to be used in the configuration", Order = 16)]
        [InputType(InputType.fieldPicker, PlatformDropdownType.multiFieldSelection)]
        public DtoFieldSelection SelectedFields { get; set; }

        [Display(Name = "Field Assigner", Description = "Showcase that demonstrates how to assign fields to be used in the configuration", Order = 17)]
        [InputType(InputType.fieldPicker, PlatformDropdownType.fieldAssignment)]
        public DtoFieldSelection AssignedFields { get; set; }

        [Display(Name = "Single Field Selector", Description = "Showcase that demonstrates how to select a single field to be used in the configuration", Order = 18)]
        [InputType(InputType.fieldPicker, PlatformDropdownType.singleFieldSelection)]
        public DtoFieldSelection SelectedField { get; set; }

        /// <summary>
        /// Configure a list to enable entering of emails and save them to list of string.
        /// </summary>
        [Display(Name = "List Of Emails", Description = "Showcase that demonstrate how to use a list as a property that accepts entering of e-mails. Variable input on this property is disabled", GroupName = "Complex properties", Order = 19)]
        [InputType(InputType.listOfValues, InputType.email)]
        [DisableVariableInput]
        public List<string> StringMultiValueProperty { get; set; }

        /// <summary>
        /// Configures a list of numbers to be entered in UI, and validation on each item that requires min length of the entry to be 2 characters
        /// </summary>
        [Display(Name = "List of integers with min length", Description = "Showcase that demonstrate how to use a list as a property that accepts entering integer numbers that are considered invalid if they are shorter than 2 digits. This sample also demonstrates that list entry type can be auto-detected Variable input on this property is disabled", GroupName = "Complex properties", Order = 20)]
        [InputType(InputType.listOfValues)]
        [MinLength(2, ErrorMessage = "Min length of list items must be 2 characters")]
        [DisableVariableInput]
        public List<int> IntMultiValueProperty { get; set; }

        /// <summary>
        /// Configures a list of numbers to be entered in UI, shows how to assign variable per list item
        /// </summary>
        [Display(Name = "List Of Doubles", Description = "Showcase that demonstrate how to use a list as a property that accepts entering of valid numbers and usage of variables on list items.", GroupName = "Complex properties", Order = 21)]
        [InputType(InputType.listOfValues, InputType.number)]
        public List<double> DoubleMultiValueProperty { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalSettings()
        {
            // Nested class must be initialized in constructor else it will not get rendered in UI!
            NestedClassProperty = new NestedWithFiles();
            //Field selection properties must be initialized in constructor else they will not get rendered in UI!
            SelectedFields = new DtoFieldSelection();
            AssignedFields = new DtoFieldSelection();
            SelectedField = new DtoFieldSelection();

            //In order to use the list properties they must be at least initialized
            StringMultiValueProperty = new List<string>();
            //You can also predefine some values in constructor if required
            StringMultiValueProperty.Add("john.doe@acme.com");
            StringMultiValueProperty.Add("jDoe@acme.com");
            StringMultiValueProperty.Add("john_doe@acme.com");

            IntMultiValueProperty = new List<int>();

            DoubleMultiValueProperty = new List<double>();
            DoubleMultiValueProperty.Add(1.1);
            DoubleMultiValueProperty.Add(2.2);
            DoubleMultiValueProperty.Add(3.3);
            DoubleMultiValueProperty.Add(4.4);
        }
    }

    /// <summary>
    /// Sample class for nested properties demonstration
    /// </summary>
    public class NestedWithFiles : ActivityConfigBase<NestedWithFiles>
    {
        /// <summary>
        /// Sample DtoFileProperty property, containing the base64 encoded file stream with additional meta-data (see DtoFileProperty's properties), rendered with file uploader
        /// </summary>
        /// <remarks>
        /// In this case you will be able to use variables for files
        /// </remarks>
        [Display(Name = "File Upload with object", Description = "File upload with File Object property that allows setting of the variables", Order = 1)]
        [InputType(InputType.file)]
        public DtoFileProperty UploadedFile { get; set; }

        [Display(Name = "Single Field Selector", Description = "Showcase that demonstrates how to select single field to be used in the configuration", Order = 2)]
        [InputType(InputType.fieldPicker)]
        //[VariableInput(true, InputType.fieldPicker)]
        public DtoFieldSelection SelectedField { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NestedWithFiles()
        {
            // DtoFileProperty type property must be initialized in constructor else it will not get rendered in UI
            UploadedFile = new DtoFileProperty();
            //Field selection property must be initialized in constructor else they will not get rendered in UI!
            SelectedField = new DtoFieldSelection();
        }
    }

}
