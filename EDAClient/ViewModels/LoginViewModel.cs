using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Input;
using EDAClient.Common;
using STG.RT.API;
using STG.RT.API.AuthExtensions;
using STG.RT.API.Interfaces;

namespace EDAClient.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "We rather have more concise code.")]
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly Func<bool, Task> _loginCallback;
        private ICommand _loginCommand;
        private ISession _session;
        private OidcAuthenticator _authProvider;
        public LoginViewModel(Func<bool, Task> loginCallback)
        {
            _loginCallback = loginCallback;
            if (string.IsNullOrEmpty(ConfigServiceURL))
            {
                ConfigServiceURL = ConfigurationManager.AppSettings["ConfigurationServiceURL"];
            }

        }

        public bool IsLoggingIn { get; set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }
        public string ConfigServiceURL { get; set; }

        public ICommand LoginCommand => _loginCommand ??
                                        (_loginCommand = new AsyncCommandExecutor(loginCommand_Execute, loginCommand_CanExecute, nameof(LoginCommand)));

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task loginCommand_Execute(object o)
        {
            if (IsLoggedIn)
            {
                await _session.LogoutAsync();
                _session = null;
                _authProvider = null;
                SharedData.ClientFactory = null;
                IsLoggedIn = false;
                await _loginCallback(false);
                return;
            }

            IsLoggingIn = true;
            await ConfigureServicesAndLogin();
        }

        private bool loginCommand_CanExecute(object o)
        {
            return CanLogin();
        }

        private async Task ConfigureServicesAndLogin()
        {
            try
            {
                var clientFactoryOptions = await GetOptionsWithConfiguredEndpoints();
                if (clientFactoryOptions == null)
                    return;
                if (await LogIn(clientFactoryOptions))
                {
                    IsLoggedIn = true;
                    SharedData.SnackBarMessageQ.Enqueue($"Welcome {UserName}");
                    await _loginCallback(true);
                }
            }
            catch (Exception ex)
            {
                SharedData.SnackBarMessageQ.Enqueue($"Failed to log in - {ex.Message}");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private async Task<ClientFactoryOptions> GetOptionsWithConfiguredEndpoints()
        {
            var discovery = new ServiceDiscovery();
            try
            {
                var endpoints = await discovery.GetServiceEndpointsAsync(ConfigServiceURL);
                var ret = new ClientFactoryOptions(endpoints);
                _authProvider = new OidcAuthenticator(endpoints.AuthenticationServiceEndpoint.Uri, "console_app", "");
                ret.UseAuthenticationProvider(_authProvider);
                return ret;
            }
            catch (Exception ex)
            {
                SharedData.SnackBarMessageQ.Enqueue($"Failed to get the service endpoints from {ConfigServiceURL} - {ex.Message}");
            }

            return null;
        }

        private async Task<bool> LogIn(ClientFactoryOptions options)
        {
            var hook = SslCertificateMonitor.HookIn(options.AuthenticationServiceEndpoint.Uri);
            try
            {
                var factory = new ClientFactory(options);
                _session = factory.GetSession();
                await _session.LoginAsync();
                UserName = _authProvider.GetTokenData().User.Identity.Name;
                SharedData.ClientFactory = factory;
                SharedData.EDA = SharedData.ClientFactory.CreateEventDrivenService();
                return true;
            }
            catch (Exception ex)
            {
                SharedData.ClientFactory = null;
                var msg = hook.IsSslError ? hook.ErrorMessage : ex.Message;
                SharedData.SnackBarMessageQ.Enqueue($"Failed to login to services due to an issue: {msg}");
                return false;
            }
            finally
            {
                hook.HookOut();
            }
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(ConfigServiceURL)) return false;

            return true;
        }
    }
}
