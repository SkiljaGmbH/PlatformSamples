using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using STG.Common.Utilities.Exceptions;
using STG.Common.Utilities.Logging;
using STG.Common.Utilities.Oidc;
using STG.RT.API.ServiceHttpClients;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Used for Password Authentication with a platform of version 3.0 or lower.
    /// </summary>
    public class LoginPasswordAuthenticator : IAuthenticationProvider
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private const string ClientId = "STG.RT.API2";
        private const string ClientSecret = "EAAAAKiun5EGqUFodwurVgDp1sR4+JlVrE/GxSfeNmkzCRwN";
        private const string PasswordGrantType = "password";
        private const string RefreshTokenGrantType = "refresh_token";

        private readonly string _authenticationServiceEndpointUri;
        private readonly string _scope = "interactive unattended";
        private readonly string _username;
        private readonly string _password;
        private readonly string _authTokenUserName;
        private readonly string _authTokenUserDisplayName;
        private AuthenticationResult _authenticationResult;

        public string RefreshToken => _authenticationResult?.RefreshToken ?? string.Empty;

        public string UserName => _authenticationResult?.Username ?? _authTokenUserName;

        public string UserDisplayName => _authenticationResult?.Username ?? _authTokenUserDisplayName;

        public LoginPasswordAuthenticator(string authenticationServiceEndpointUri, string username, string password, string refreshToken, string authTokenUserName, string authTokenUserDisplayName)
        {
            _authenticationServiceEndpointUri = authenticationServiceEndpointUri;
            _username = username;
            _password = password;
            _authenticationResult = string.IsNullOrEmpty(refreshToken) ? null : new AuthenticationResult() { RefreshToken = refreshToken };
            _authTokenUserName = authTokenUserName;
            _authTokenUserDisplayName = authTokenUserDisplayName;
            // adjust the scope to your needs
            // _scope = "interactive";
        }


        public async ValueTask<string> GetTokenAsync()
        {
            return await new ValueTask<string>(GetToken());
        }

        public async ValueTask<AuthenticationResult> GetAuthenticationResultAsync()
        {
            return await new ValueTask<AuthenticationResult>(_authenticationResult);
        }

        private string GetToken()
        {
            return _authenticationResult?.AccessToken;
        }

        public async ValueTask LogoutAsync()
        {
            _authenticationResult = null;
            await new ValueTask();
        }

        public async ValueTask<string> PingAsync()
        {
            var uriBuilder = new UriBuilder(_authenticationServiceEndpointUri);
            uriBuilder.Path += "api/auth/ping";
            var tokenUri = uriBuilder.Uri;

            try
            {
                using (var httpClient = HttpClientFactory.CreateHttpClient())
                {
                    using (var httpResponseMessage = await httpClient.GetAsync(tokenUri))
                    {
                        var content = await httpResponseMessage.Content.ReadAsStringAsync();
                        _logger.Trace($"PingAsync response {content}");
                        return content;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("PingAsync", e);
                return null;
            }
        }

        public async ValueTask<string> UpdateTokenAsync(string oldToken)
        {
            if (_authenticationResult == null)
            {
                return await AuthenticateAsync();
            }
            else
            {
                return await RefreshTokenAsync();
            }
        }

        public async Task<string> AuthenticateAsync()
        {
            try
            {
                var httpContentKeyValuePairList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", PasswordGrantType),
                    new KeyValuePair<string, string>("machine_name", Environment.MachineName),
                    new KeyValuePair<string, string>("username", _username),
                    new KeyValuePair<string, string>("password", _password)
                };

                if (string.IsNullOrEmpty(_scope) == false)
                {
                    httpContentKeyValuePairList.Add(new KeyValuePair<string, string>("scope", _scope));
                }

                var httpContent = new FormUrlEncodedContent(httpContentKeyValuePairList);

                _authenticationResult = await FetchAuthorizationTokenAsync(httpContent);

                // you might want to redact the username.
                _logger.TraceFormat("Authenticated user {username} with grant_type {granttype}", _authenticationResult.Username, PasswordGrantType);

                return _authenticationResult.AccessToken;
            }
            catch (Exception e)
            {
                _logger.ErrorException("AuthenticateAsync", e);
                throw;
            }
        }

        public async Task<string> RefreshTokenAsync()
        {
            try
            {
                var formUrlEncodedContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", RefreshTokenGrantType),
                    new KeyValuePair<string, string>("refresh_token", _authenticationResult.RefreshToken)
                });

                _authenticationResult = await FetchAuthorizationTokenAsync(formUrlEncodedContent);

                return _authenticationResult.AccessToken;
            }
            catch (STGUnauthorizedException e)
            {
                _logger.ErrorException("Using the refresh token failed.", e);
                _authenticationResult = null;
                return await AuthenticateAsync();
            }
        }

        private async Task<AuthenticationResult> FetchAuthorizationTokenAsync(HttpContent httpContent)
        {
            var uriBuilder = new UriBuilder(_authenticationServiceEndpointUri);
            uriBuilder.Path += "token";
            var tokenUri = uriBuilder.Uri;

            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                using (var httpClient = HttpClientFactory.CreateHttpClient())
                {
                    using (var httpResponseMessage = await httpClient.PostAsync(tokenUri, httpContent))
                    {
                        var isUnauthorized = await IsUnauthorized(httpResponseMessage);
                        if (isUnauthorized) throw new STGUnauthorizedException($"{httpResponseMessage.ReasonPhrase}");

                        httpStatusCode = httpResponseMessage.StatusCode;
                        if (!httpResponseMessage.IsSuccessStatusCode) throw new OwinServiceException($"Authentication token request failed. StatusCode: {httpResponseMessage.StatusCode}. Uri: {tokenUri}.");

                        var content = await httpResponseMessage.Content.ReadAsStringAsync();
                        var authenticationResult = JsonConvert.DeserializeObject<AuthenticationResult>(content);

                        authenticationResult.Name = authenticationResult.Username;

                        return authenticationResult;
                    }
                }
            }
            catch (STGCommunicationException e)
            {
                _logger.ErrorException("FetchAuthorizationTokenAsync", e);

                var message = string.Empty;
                if (httpStatusCode == HttpStatusCode.NotFound)
                {
                    message = $" The status code 404 (Not Found) can happen if the target service is not running and another service tries to answer. Uri: {tokenUri}.";
                }
                else if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    message = $" The status code 503 (Service Unavailable) happens if the target service is not running on the target machine at that Uri. Uri: {tokenUri}.";
                }

                throw new STGCommunicationException(ErrorCode.AuthenticationServiceError, $"Authentication failed. StatusCode {httpStatusCode}.{message}", e);
            }
            catch (OwinServiceException e)
            {
                // This is old - starting with 2.4.4, the api would return STGCommunicationExceptions directly.
                if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    var message = string.Empty;
                    if (httpStatusCode == HttpStatusCode.NotFound)
                    {
                        message = $" The status code 404 (Not Found) can happen if the target service is not running and another service tries to answer. Uri: {tokenUri}.";
                    }
                    else if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        message = $" The status code 503 (Service Unavailable) happens if the target service is not running on the target machine at that Uri. Uri: {tokenUri}.";
                    }

                    throw new STGCommunicationException(ErrorCode.AuthenticationServiceError, $"Authentication failed. StatusCode {httpStatusCode}.{message}", e);
                }

                throw;
            }
        }

        private async Task<bool> IsUnauthorized(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.IsSuccessStatusCode) return false;

            try
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                _logger.Trace($"Response message content {content}");

                var errorContent = JsonConvert.DeserializeObject<ErrorContent>(content);

                if (!string.IsNullOrEmpty(errorContent?.error) && errorContent.error == "invalid_grant" &&
                    !string.IsNullOrEmpty(errorContent?.error_description) && errorContent.error_description.StartsWith("Login failed"))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("Error deserializing content", e);
            }

            return false;
        }

        public ValueTask<string> GetUsernameAsync()
        {
            var idToken = _authenticationResult?.GetIdentityToken();
            return new ValueTask<string>(idToken?.Username ?? "unknown");
        }
    }
}
