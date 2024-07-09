using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;

namespace ActivitySettings
{
    public class DatabaseConnection : ActivityConfigBase<DatabaseConnection>
    {
        [InputType(STG.Common.DTO.Metadata.InputType.text)]
        [Display(Name = "Server name", Order = 1)]
        [Required(ErrorMessage = "Data is required.")]
        public string ServerName
        {
            get;
            set;
        }

        [InputType(STG.Common.DTO.Metadata.InputType.text)]
        [Display(Name = "Database name", Order = 2)]
        [Required(ErrorMessage = "Data is required.")]
        public string DatabaseName
        {
            get;
            set;
        }

        [InputType(STG.Common.DTO.Metadata.InputType.checkbox)]
        [Display(Name = "Integrated security", Order = 3)]
        public bool IntegratedSecurity
        {
            get;
            set;
        }

        [InputType(STG.Common.DTO.Metadata.InputType.text)]
        [Display(Name = "Username", Order = 4)]
        public string Username
        {
            get;
            set;
        }

        [InputType(STG.Common.DTO.Metadata.InputType.password)]
        [Display(Name = "Password", Order = 5)]
        public string Password
        {
            get;
            set;
        }

        public string GetDatabaseConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = ServerName;
            builder["Initial Catalog"] = DatabaseName;
            builder["Integrated Security"] = IntegratedSecurity;
            builder["User Id"] = Username;
            builder["Password"] = Password;

            return builder.ConnectionString;
        }

        public DatabaseConnection()
        {
            ServerName = "[sql server name]";
            DatabaseName = "[database name]";
            Username = "[username]";
            Password = "[password]";
        }
    }
}
