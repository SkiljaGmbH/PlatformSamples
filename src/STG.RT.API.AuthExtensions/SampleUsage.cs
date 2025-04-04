using System;
using System.Threading.Tasks;

namespace STG.RT.API.AuthExtensions
{
    public class SampleUsage
    {
        public async Task Main(string configurationServiceEndpoint)
        {
            var serviceEndpoints = await new ServiceDiscovery().GetServiceEndpointsAsync(configurationServiceEndpoint);
            if (serviceEndpoints == null)
                throw new InvalidOperationException("The service endpoints discovery returned null - this usually means that the configuration service does not exist at this URL.");

            var clientFactoryOptions = new ClientFactoryOptions(serviceEndpoints);

            // verify the platform version - 3.1 is ready to use the authorization code flow.
            // Other flows are not supported, the client credentials flow works out of the box
            // 3.0 only supports the OAuth2 password flow or the custom windows flow.
            var platformVersion = await PlatformVersionChecker.GetPlatformVersionAsync(configurationServiceEndpoint);
            if (platformVersion >= new Version(3, 1))
            {
                clientFactoryOptions.UseAuthenticationProvider(new OidcAuthenticator(serviceEndpoints.AuthenticationServiceEndpoint.Uri, "console_app", ""));
            }
            else if (UseIntegratedWindowsAuthentication)
            {
                clientFactoryOptions.UseAuthenticationProvider(new WindowsAuthenticator(serviceEndpoints.WindowsAuthenticationServiceEndpoint.Uri.ToString(), serviceEndpoints.AuthenticationServiceEndpoint.Uri.ToString(), "1234", "john.doe@contoso.com", "John Doe"));
            }
            else
            {
                clientFactoryOptions.UseAuthenticationProvider(new LoginPasswordAuthenticator(
                    serviceEndpoints.AuthenticationServiceEndpoint.Uri.ToString(), "username", "password", "1234",
                    "john.doe@contoso.com", "John Doe"));
            }
        }

        /// <summary>
        /// Set to true to log in with a Kerberos Ticket or NTLM token (depends on how Windows and its domain are configured)
        /// </summary>
        public bool UseIntegratedWindowsAuthentication { get; set; }
    }
}
