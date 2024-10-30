using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using STG.Common.Utilities.Exceptions;
using STG.Common.Utilities.Logging;
using STG.Common.Utilities.Oidc;
using STG.RT.API.ServiceHttpClients;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Used for Integration Windows Authentication with a platform of version 3.0 or lower.
    /// </summary>
    public class WindowsAuthenticator : IAuthenticationProvider, IAuthenticationResult
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private const string ClientId = "STG.RT.API2";
        private const string ClientSecret = "EAAAAKiun5EGqUFodwurVgDp1sR4+JlVrE/GxSfeNmkzCRwN";
        private const string WindowsGrantType = "windows";
        private const string RefreshTokenGrantType = "refresh_token";

        private readonly string _windowsAuthenticationServiceEndpointUri;
        private readonly string _authenticationServiceEndpointUri;
        private readonly string _scope = "interactive unattended";
        private AuthenticationResult _authenticationResult;
        private WinAuthJwt _winAuthJwt;
        private readonly string _authTokenUserName;
        private readonly string _authTokenUserDisplayName;

        public string RefreshToken => _authenticationResult?.RefreshToken ?? string.Empty;

        public string UserName => _authenticationResult?.Username ?? _authTokenUserName;

        public string UserDisplayName => _authenticationResult?.Username ?? _authTokenUserDisplayName;

        public WindowsAuthenticator(string windowsAuthenticationServiceEndpointUri, string authenticationServiceEndpointUri, string refreshToken, string authTokenUserName, string authTokenUserDisplayName)
        {
            _windowsAuthenticationServiceEndpointUri = windowsAuthenticationServiceEndpointUri;
            _authenticationServiceEndpointUri = authenticationServiceEndpointUri;
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

        public async ValueTask<string> PingAsync()
        {
            var uriBuilder = new UriBuilder(_windowsAuthenticationServiceEndpointUri);
            uriBuilder.Path += "api/ping";
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

        public async ValueTask LogoutAsync()
        {
            _authenticationResult = null;
            await new ValueTask();
        }

        private string GetToken()
        {
            return _authenticationResult?.AccessToken;
        }

        private async Task<string> AuthenticateAsync()
        {
            var httpContent = await GetAuthenticationRequestContentAsync();

            try
            {
                _authenticationResult = await FetchAuthorizationTokenAsync(httpContent);
            }
            catch (Exception e)
            {
                _logger.ErrorException("AuthenticateAsync", e);
                throw;
            }

            _logger.TraceFormat("Authenticated user {username} with grant_type {grantType}", _authenticationResult.Username, WindowsGrantType);

            return _authenticationResult.AccessToken;
        }

        private async Task<HttpContent> GetAuthenticationRequestContentAsync()
        {
            var httpContentKeyValuePairList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("machine_name", Environment.MachineName),
                new KeyValuePair<string, string>("grant_type", WindowsGrantType)
            };

            if (string.IsNullOrEmpty(_scope) == false)
            {
                httpContentKeyValuePairList.Add(new KeyValuePair<string, string>("scope", _scope));
            }

            if (_winAuthJwt == null || _winAuthJwt.ExpiresUtc <= DateTime.UtcNow)
            {
                _winAuthJwt = await AuthenticateWithWindowsAuthenticationAsync();
            }

            httpContentKeyValuePairList.Add(new KeyValuePair<string, string>("win_token", _winAuthJwt.access_token));

            return new LargeStringFormUrlEncodedContent(httpContentKeyValuePairList);
        }

        private async Task<string> RefreshTokenAsync()
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

        private async Task<WinAuthJwt> AuthenticateWithWindowsAuthenticationAsync()
        {
            var formUrlEncodedContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", WindowsGrantType),
                new KeyValuePair<string, string>("machine_name", Environment.MachineName)
            });

            var uriBuilder = new UriBuilder(_windowsAuthenticationServiceEndpointUri);
            uriBuilder.Path += "token";
            var tokenUri = uriBuilder.Uri;

            try
            {
                using (var httpClient = HttpClientFactory.CreateHttpClient())
                {
                    using (var httpResponseMessage = await httpClient.PostAsync(tokenUri, formUrlEncodedContent))
                    {
                        if (!httpResponseMessage.IsSuccessStatusCode)
                        {
                            throw new Exception($"Windows authentication failed - response StatusCode: {httpResponseMessage.StatusCode}. Uri: {tokenUri}.");
                        }

                        var content = await httpResponseMessage.Content.ReadAsStringAsync();
                        var winAuthJwt = JsonConvert.DeserializeObject<WinAuthJwt>(content);
                        if (string.IsNullOrEmpty(winAuthJwt.access_token))
                        {
                            throw new STGCommunicationException(ErrorCode.WindowsAuthenticationFailed, $"Windows authentication failed - response did not contain a token. Uri: {tokenUri}.");
                        }

                        winAuthJwt.ExpiresUtc = DateTime.UtcNow.AddSeconds(winAuthJwt.expires_in);

                        return winAuthJwt;
                    }
                }
            }
            catch (STGCommunicationException e)
            {
                // happens when:
                //      a) the service isn't running and no other service answers the call with 404: Unable to connect to the remote server 
                //      b) the firewall blocks the request: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server. 
                //      c) The remote name could not be resolved: 'zippvm2.de.skilja.com'
                _logger.ErrorException("AuthenticateWithWindowsAuthenticationAsync", e);
                throw new STGCommunicationException(ErrorCode.WindowsAuthenticationServiceError, $"Windows authentication failed. StatusCode: {e.ResponseCode}. Message: {e.Message}. Uri: {tokenUri}.", e);
            }
            catch (Exception e)
            {
                // not sure if this ever happens. But we'll have to handle that eventuality gracefully
                _logger.ErrorException("AuthenticateWithWindowsAuthenticationAsync", e);
                throw new STGCommunicationException(ErrorCode.WindowsAuthenticationServiceError, $"Windows authentication failed. Uri: {tokenUri}.", e);
            }
        }

        /// <summary>
        /// Gets the token from the authorization server.
        /// </summary>
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

                throw new STGCommunicationException(ErrorCode.AuthenticationServiceError, string.Format($"Authentication failed. StatusCode: {httpStatusCode}.{message}"), e);
            }
            catch (OwinServiceException e)
            {
                _logger.ErrorException("FetchAuthorizationTokenAsync", e);

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

                    throw new STGCommunicationException(ErrorCode.AuthenticationServiceError, string.Format($"Authentication failed. StatusCode {httpStatusCode}.{message}"), e);
                }

                throw;
            }
        }

        public ValueTask<string> GetUsernameAsync()
        {
            // this is not supported on 3.1/4.0 anymore - 3.0 returns the username differently.
            return new ValueTask<string>(_authenticationResult?.Name ?? "Unknown");
        }

        /// <summary>
        /// Content class for data in strings larger than 65520 characters.
        /// Use instead of FormUrlEncodedContent when content is too big
        /// </summary>
        public class LargeStringFormUrlEncodedContent : ByteArrayContent
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="nameValueCollection"></param>
            public LargeStringFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
                : base(GetContentByteArray(nameValueCollection))
            {
                base.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }

            public static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            {
                if (nameValueCollection == null)
                {
                    throw new ArgumentNullException("nameValueCollection");
                }
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeyValuePair<string, string> item in nameValueCollection)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('&');
                    }
                    stringBuilder.Append(Encode(item.Key));
                    stringBuilder.Append('=');
                    stringBuilder.Append(Encode(item.Value));
                }
                return Encoding.UTF8.GetBytes(stringBuilder.ToString());
            }

            public static string Encode(string data)
            {
                if (string.IsNullOrEmpty(data))
                {
                    return string.Empty;
                }
                return WebUtility.UrlEncode(data).Replace("%20", "+");
            }
        }
    }
}
