using STG.Common.DTO.Metadata;
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
        [Display(Name = nameof(Resources.NestedWithFiles_UploadedFile_Name), Description = nameof(Resources.NestedWithFiles_UploadedFile_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.file)]
        public DtoFileProperty UploadedFile { get; set; }

        [Display(Name = nameof(Resources.NestedWithFiles_SelectedField_Name), Description = nameof(Resources.NestedWithFiles_SelectedField_Description), Order = 2, ResourceType = typeof(Resources))]
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