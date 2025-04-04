using System.ComponentModel.DataAnnotations;
using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using Microsoft.Data.SqlClient;

namespace SampleActivityWithDependency;

public class SQLClientCheck : STGUnattendedAbstract<SQLClientCheckConfig>
{
    public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
    {
        SqlConnection? conn = null;
        try
        {
            string connectionString = $"Server={ActivityConfiguration.ServerName};Database={ActivityConfiguration.DatabaseName};"
            + "User ID = {ActivityConfiguration.DBUsername}; Password ={ActivityConfiguration.DBPassword};"
            + "Integrated Security={ActivityConfiguration.UseIntegratedSecurity};"
            + "TrustServerCertificate={ActivityConfiguration.UseTrustServerCertificate};";

            // The SqlConnection is a runtime-dependent class.
            // In .NET 8, if the Microsoft.Data.SqlClient directly from the output directory is used
            // then this call will fail - that library is only a reference assembly.
            // The correct compilation assembly must be used from the runtimes folder.
            // The SampleActivityWithDependency.deps.json file describes to the .NET runtime what library to pick.
            conn = new SqlConnection(connectionString);

            conn.Open();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            conn?.Close();
        }
    }
}
public class SQLClientCheckConfig : ActivityConfigBase<SQLClientCheckConfig>
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Server name", Description = "Enter the server name for the database server installation. To provide a specifically configured port number enter: <server name>:<port number>.\r\n", Order = 2)]
    [InputType(InputType.text)]
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Database name", Description = "Enter the database name. For Oracle, leave the field empty.", Order = 3)]
    [InputType(InputType.text)]
    public string DatabaseName { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    [InputType(STG.Common.DTO.Metadata.InputType.checkbox)]
    [Display(Name = "Use integrated security", Description = "Select this option when the platform services user has the necessary database permissions. Alternatively to using this option, you can provide a user name and password.", Order = 4)]
    public bool UseIntegratedSecurity { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Username", Description = "Enter the name of the user when you do not use integrated security.", Order = 5)]
    [InputType(InputType.text)]
    public string DBUsername { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Password", Description = "Enter the password for the specified user.", Order = 6)]
    [InputType(InputType.password)]
    public string DBPassword { get; set; } = string.Empty;

    [Display(Name = "Trust server certificate", Description = "Whether or not to implicitly trust the server certificate that is used for channel encryption.", Order = 8)]
    [InputType(InputType.checkbox)]
    public bool UseTrustServerCertificate { get; set; }
}