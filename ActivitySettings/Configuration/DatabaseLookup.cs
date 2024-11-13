using STG.Common.DTO;
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
    public class DatabaseLookup : ActivityConfigBase<DatabaseLookup>
    {
        [Display(Name = nameof(Resources.DatabaseLookup_Name_Name), Description = nameof(Resources.DatabaseLookup_Name_Description), Order = 1, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_TableName_Name), Description = nameof(Resources.DatabaseLookup_TableName_Description), Order = 2, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string TableName { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_Columns_Name), Description = nameof(Resources.DatabaseLookup_Columns_Description), Order = 3, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        [Required]
        public string Columns { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_FullTextSearch_Name), Description = nameof(Resources.DatabaseLookup_FullTextSearch_Description), Order = 4, ResourceType = typeof(Resources))]
        [InputType(InputType.checkbox)]
        public bool FullTextSearch { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_FullTextColumns_Name), Description = nameof(Resources.DatabaseLookup_FullTextColumns_Description), Order = 5, ResourceType = typeof(Resources))]
        [InputType(InputType.text)]
        public string FullTextColumns { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_MaxResults_Name), Description = nameof(Resources.DatabaseLookup_MaxResults_Description), Order = 7, ResourceType = typeof(Resources))]
        [InputType(InputType.number)]
        [Required]
        public int MaxResults { get; set; }

        [Display(Name = nameof(Resources.DatabaseLookup_DatabaseConnection_Name), Description = nameof(Resources.DatabaseLookup_DatabaseConnection_Description), Order = 8, ResourceType = typeof(Resources))]
        [InputType(InputType.nestedClass)]
        [Required]
        public DatabaseConnection DatabaseConnection { get; set; }

        public DatabaseLookup()
        {
            Name = "";
            TableName = "";
            Columns = string.Empty;
            MaxResults = 100;
            FullTextSearch = false;
            DatabaseConnection = new DatabaseConnection();
        }
    }
}