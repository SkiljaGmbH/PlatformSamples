using STG.Common.DTO.Metadata;
using STG.Common.DTO;
using STG.RT.API.Activity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivitySettings.Properties;

namespace ActivitySettings.Configuration
{
    /// <summary>
    /// Sample settings
    /// </summary>
    public class ExternalSettings : ActivityConfigBase<ExternalSettings>
    {
        /// <summary>
        /// Sample string property rendered with one line text box
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_StringProperty_Name), Description = nameof(Resources.ExternalSettings_StringProperty_Description), GroupName = nameof(Resources.ExternalSettings_StringProperty_GroupName), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string StringProperty { get; set; }

        /// <summary>
        /// Sample password property 
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_PwdProperty_Name), Description = nameof(Resources.ExternalSettings_PwdProperty_Description), GroupName = nameof(Resources.ExternalSettings_PwdProperty_GroupName), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.password)]
        public string PwdProperty { get; set; }

        /// <summary>
        /// Sample string property rendered with multi line text area
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_LongStringProperty_Name), Description = nameof(Resources.ExternalSettings_LongStringProperty_Description), GroupName = nameof(Resources.ExternalSettings_LongStringProperty_GroupName), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.textarea)]
        public string LongStringProperty { get; set; }

        /// <summary>
        /// Sample double property 
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_DoubleProperty_Name), Description = nameof(Resources.ExternalSettings_DoubleProperty_Description), GroupName = nameof(Resources.ExternalSettings_DoubleProperty_GroupName), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        public double DoubleProperty { get; set; }

        /// <summary>
        /// Sample boolean property rendered with check box
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_BooleanProperty_Name), Description = nameof(Resources.ExternalSettings_BooleanProperty_Description), GroupName = nameof(Resources.ExternalSettings_BooleanProperty_GroupName), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        public bool BooleanProperty { get; set; }

        /// <summary>
        /// Sample date property rendered with date picker
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_DateTimeProperty_Name), Description = nameof(Resources.ExternalSettings_DateTimeProperty_Description), GroupName = nameof(Resources.ExternalSettings_DateTimeProperty_GroupName), Order = 6, ResourceType = typeof(Resources))]
        [InputType(InputType.date)]
        public DateTime DateTimeProperty { get; set; }

        /// <summary>
        /// Sample color property rendered with color picker
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_ColorProperty_Name), Description = nameof(Resources.ExternalSettings_ColorProperty_Description), GroupName = nameof(Resources.ExternalSettings_ColorProperty_GroupName), Order = 7, ResourceType = typeof(Resources))]
        [InputType(InputType.colorpicker)]
        public string ColorProperty { get; set; }

        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with enumeration values
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_EnumPropertyCombo_Name), Description = nameof(Resources.ExternalSettings_EnumPropertyCombo_Description), GroupName = nameof(Resources.ExternalSettings_EnumPropertyCombo_GroupName), Order = 8, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, typeof(EnumToDemo))]
        public EnumToDemo EnumPropertyCombo { get; set; }

        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with provided key-value pair values
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_IntegerCombo_Name), Description = nameof(Resources.ExternalSettings_IntegerCombo_Description), GroupName = nameof(Resources.ExternalSettings_IntegerCombo_GroupName), Order = 9, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, @"[{'Key':1,'Value':'One'},{'Key':2,'Value':'Two'},{'Key':3,'Value':'Three'}]")]
        public int IntegerCombo { get; set; }

        /// <summary>
        /// Sample enumeration property rendered with drop down list filled with provided string values
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_CommaDelimitedCombo_Name), Description = nameof(Resources.ExternalSettings_CommaDelimitedCombo_Description), GroupName = nameof(Resources.ExternalSettings_CommaDelimitedCombo_GroupName), Order = 10, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, @"Option A;Option B;Option C")]
        public string CommaDelimitedCombo { get; set; }

        /// <summary>
        /// Sample property showing how to use the document type combo selection
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_DocumentTypeSelection_Name), Description = nameof(Resources.ExternalSettings_DocumentTypeSelection_Description), GroupName = nameof(Resources.ExternalSettings_DocumentTypeSelection_GroupName), Order = 11, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, PlatformDropdownType.document)]
        public string DocumentTypeSelection { get; set; }

        /// <summary>
        /// Sample property showing how to use the media type combo selection
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_MediaTypeSelection_Name), Description = nameof(Resources.ExternalSettings_MediaTypeSelection_Description), GroupName = nameof(Resources.ExternalSettings_MediaTypeSelection_GroupName), Order = 12, ResourceType = typeof(Resources))]
        [InputType(InputType.enumeration, PlatformDropdownType.media)]
        public string MediaTypeSelection { get; set; }

        /// <summary>
        /// Sample string property, containing only the base64 encoded file stream, rendered with file uploader.
        /// </summary>
        /// <remarks>
        /// Although this sample allows variable on this field, this setting will be ignored when UI gets rendered.
        /// The reason is that variables on files can be used only if type of the property is DtoFileProperty
        /// </remarks>
        [Display(Name = nameof(Resources.ExternalSettings_FileUploadProperty_Name), Description = nameof(Resources.ExternalSettings_FileUploadProperty_Description), GroupName = nameof(Resources.ExternalSettings_FileUploadProperty_GroupName), Order = 13, ResourceType = typeof(Resources))]
        [InputType(InputType.file)]
        public string FileUploadProperty { get; set; }

        /// <summary>
        /// Sample dictionary property rendered with key-value pair grid
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_GridProperty_Name), Description = nameof(Resources.ExternalSettings_GridProperty_Description), GroupName = nameof(Resources.ExternalSettings_GridProperty_GroupName), Order = 14, ResourceType = typeof(Resources))]
        [InputType(InputType.dictionary)]
        public SerializableDictionary<string, string> GridProperty { get; set; }

        /// <summary>
        /// Sample custom nested class property rendered it its own group with the class's properties
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_NestedClassProperty_Name), Description = nameof(Resources.ExternalSettings_NestedClassProperty_Description), Order = 15, ResourceType = typeof(Resources))]
        [InputType(InputType.nestedClass)]
        public NestedWithFiles NestedClassProperty { get; set; }

        [Display(Name = nameof(Resources.ExternalSettings_SelectedFields_Name), Description = nameof(Resources.ExternalSettings_SelectedFields_Description), Order = 16, ResourceType = typeof(Resources))]
        [InputType(InputType.fieldPicker, PlatformDropdownType.multiFieldSelection)]
        public DtoFieldSelection SelectedFields { get; set; }

        [Display(Name = nameof(Resources.ExternalSettings_AssignedFields_Name), Description = nameof(Resources.ExternalSettings_AssignedFields_Description), Order = 17, ResourceType = typeof(Resources))]
        [InputType(InputType.fieldPicker, PlatformDropdownType.fieldAssignment)]
        public DtoFieldSelection AssignedFields { get; set; }

        [Display(Name = nameof(Resources.ExternalSettings_SelectedField_Name), Description = nameof(Resources.ExternalSettings_SelectedField_Description), Order = 18, ResourceType = typeof(Resources))]
        [InputType(InputType.fieldPicker, PlatformDropdownType.singleFieldSelection)]
        public DtoFieldSelection SelectedField { get; set; }

        /// <summary>
        /// Configure a list to enable entering of emails and save them to list of string.
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_StringMultiValueProperty_Name), Description = nameof(Resources.ExternalSettings_StringMultiValueProperty_Description), GroupName = nameof(Resources.ExternalSettings_StringMultiValueProperty_GroupName), Order = 19, ResourceType = typeof(Resources))]
        [InputType(InputType.listOfValues, InputType.email)]
        [DisableVariableInput]
        public List<string> StringMultiValueProperty { get; set; }

        /// <summary>
        /// Configures a list of numbers to be entered in UI, and validation on each item that requires min length of the entry to be 2 characters
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_IntMultiValueProperty_Name), Description = nameof(Resources.ExternalSettings_IntMultiValueProperty_Description), GroupName = nameof(Resources.ExternalSettings_IntMultiValueProperty_GroupName), Order = 20, ResourceType = typeof(Resources))]
        [InputType(InputType.listOfValues)]
        [MinLength(2, ErrorMessageResourceName = "ExternalSettings_IntMultiValueProperty_MinLengthAttribute_ErrorMessage", ErrorMessageResourceType = typeof(Resources))]
        [DisableVariableInput]
        public List<int> IntMultiValueProperty { get; set; }

        /// <summary>
        /// Configures a list of numbers to be entered in UI, shows how to assign variable per list item
        /// </summary>
        [Display(Name = nameof(Resources.ExternalSettings_DoubleMultiValueProperty_Name), Description = nameof(Resources.ExternalSettings_DoubleMultiValueProperty_Description), GroupName = nameof(Resources.ExternalSettings_DoubleMultiValueProperty_GroupName), Order = 21, ResourceType = typeof(Resources))]
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
}