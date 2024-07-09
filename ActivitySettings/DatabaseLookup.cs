using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivitySettings
{
    public class DatabaseLookup : ActivityConfigBase<DatabaseLookup>
    {
        [Display(Name = "Name", Description = "Lookup name", Order = 1)]
        [InputType(InputType.text)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Table name", Description = "Table name", Order = 2)]
        [InputType(InputType.text)]
        [Required]
        public string TableName { get; set; }

        [Display(Name = "Table Columns", Description = "Columns with column display name", Order = 3)]
        [InputType(InputType.text)]
        [Required]
        public string Columns { get; set; }

        [Display(Name = "Allow full text search", Description = "Support for full text search", Order = 4)]
        [InputType(InputType.checkbox)]
        public bool FullTextSearch { get; set; }

        [Display(Name = "Full text search columns (comma-separated)", Description = "Columns used in full text search delimited by comma", Order = 5)]
        [InputType(InputType.text)]
        public string FullTextColumns { get; set; }

        [Display(Name = "Max returned results", Description = "Maximum number of results returned", Order = 7)]
        [InputType(InputType.number)]
        [Required]
        public int MaxResults { get; set; }

        [Display(Name = "Database Connection", Description = "Database connection settings", Order = 8)]
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
