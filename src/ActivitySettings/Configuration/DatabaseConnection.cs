using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using ActivitySettings.Properties;

namespace ActivitySettings.Configuration
{
    public class DatabaseConnection : ActivityConfigBase<DatabaseConnection>
    {
        [InputType(InputType.text)]
        [Display(Name = nameof(Resources.DatabaseConnection_ServerName_Name), Order = 1, ResourceType = typeof(Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.DatabaseConnection_ServerName_RequiredAttribute_ErrorMessage))]
        public string ServerName { get; set; }

        [InputType(InputType.text)]
        [Display(Name = nameof(Resources.DatabaseConnection_DatabaseName_Name), Order = 2, ResourceType = typeof(Resources))]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.DatabaseConnection_DatabaseName_RequiredAttribute_ErrorMessage))]
        public string DatabaseName { get; set; }

        [InputType(InputType.checkbox)]
        [Display(Name = nameof(Resources.DatabaseConnection_IntegratedSecurity_Name), Order = 3, ResourceType = typeof(Resources))]
        public bool IntegratedSecurity { get; set; }

        [InputType(InputType.text)]
        [Display(Name = nameof(Resources.DatabaseConnection_Username_Name), Order = 4, ResourceType = typeof(Resources))]
        public string Username { get; set; }

        [InputType(InputType.password)]
        [Display(Name = nameof(Resources.DatabaseConnection_Password_Name), Order = 5, ResourceType = typeof(Resources))]
        public string Password { get; set; }

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