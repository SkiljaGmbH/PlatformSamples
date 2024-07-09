using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using STG.Common.Utilities.Oidc;
using STG.RT.API.ServiceHttpClients;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Implements the OIDC authorization code flow for clients.
    /// Note: This does not work for non-interactive applications.
    /// </summary>
    public sealed class OidcAuthenticator : IAuthenticationProvider, IDisposable
    {
        private readonly Uri _authority;
        private readonly HttpMessageHandler _handler;
        private readonly bool _handlerWasCreated;
        private readonly IBrowser _browser;
        private readonly ILogger<OidcAuthenticator> _logger;
        private OidcClient _client;
        private TokenData _tokenData;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="OidcAuthenticator"/>
        /// </summary>
        /// <param name="authorityAddress">The URI of the authentication authority</param>
        /// <param name="clientId">The required client ID of the application. Client secrets for such public applications aren't currently supported.</param>
        /// <param name="handler">An optional inner handler that is used for the authentication flow. The given handler will not be disposed.</param>
        /// <param name="browser">An optional implementation of <see cref="IBrowser"/>. If <c>null</c>, a default implementation will be used.</param>
        /// <param name="existingRefreshToken">If the application remembered the previous refresh token, it is used (if set) to silently log the user in.</param>
        /// <param name="scopes">Scopes allow to add to the scopes used by default: "openid offline_access profile".</param>
        /// <param name="logger">If the logger is set, the authentication client will write logs.</param>
        /// <exception cref="ArgumentNullException">The arguments authority and clientId must bet set.</exception>
        public OidcAuthenticator(Uri authorityAddress, string clientId, string scopes, HttpMessageHandler handler = null, IBrowser browser = null, string existingRefreshToken = null, ILogger<OidcAuthenticator> logger = null)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException($"'{nameof(clientId)}' cannot be null or empty.", nameof(clientId));
            }

            _authority = authorityAddress ?? throw new ArgumentNullException(nameof(authorityAddress));
            _handler = handler;
            if (_handler == null)
            {
                _handler = new HttpClientHandler();
                _handlerWasCreated = true;
            }

            _browser = browser;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OidcAuthenticator>.Instance;
            var random = GetRandomUnusedPort();
            string redirectUri = $"http://127.0.0.1:{random}/";

            var scope = "openid offline_access profile";
            if (string.IsNullOrEmpty(scopes))
                scope += " " + scopes;

            var options = new OidcClientOptions
            {
                Authority = authorityAddress.ToString(),
                BackchannelHandler = handler,
                RefreshTokenInnerHttpHandler = handler,
                RedirectUri = redirectUri,
                ClientId = clientId,
                Scope = scope,
            };

            _tokenData = new TokenData { RefreshToken = existingRefreshToken };
            _client = new OidcClient(options);
        }

        /// <summary>
        /// Gets or sets a timeout when asking for a token
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Returns the refresh token, if it is set. Note that this method is not threadable
        /// </summary>
        public string RefreshToken => _tokenData?.RefreshToken;

        /// <summary>
        /// Gets the currently set token, either from the login or from the last token refresh.
        /// </summary>
        /// <returns>The current access token</returns>
        public async ValueTask<string> GetTokenAsync()
        {
            if (await _lock.WaitAsync(Timeout).ConfigureAwait(false))
            {
                try
                {
                    return GetToken();
                }
                finally
                {
                    _lock.Release(1);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the entire token structure, including access, refresh and identity token.
        /// </summary>
        public TokenData GetTokenData()
        {
            try
            {
                if (_lock.Wait(Timeout))
                {
                    return _tokenData;
                }
            }
            finally
            {
                _lock.Release(1);
            }

            return null;
        }

        /// <summary>
        /// Gets the current token. This method is not guarded by a lock
        /// </summary>
        private string GetToken()
        {
            return _tokenData?.AccessToken;
        }

        /// <summary>
        /// Logs the application out.
        /// </summary>
        public async ValueTask LogoutAsync()
        {
            _tokenData = null;
            await _client.LogoutAsync();
        }

        public async ValueTask<string> UpdateTokenAsync(string oldToken)
        {
            if (await _lock.WaitAsync(Timeout).ConfigureAwait(false))
            {
                try
                {

                    // if the oldToken is not the current token, then we just refreshed the tokens, so we should just return the current refresh token
                    if (string.IsNullOrEmpty(oldToken) == false && oldToken != GetToken())
                    {
                        return _tokenData?.AccessToken;
                    }

                    if (string.IsNullOrEmpty(_tokenData.RefreshToken))
                    {
                        _client.Options.Browser = _browser ?? new Browser(_authority);
                        await AuthenticateAsync();
                    }
                    else
                    {
                        await RefreshTokenAsync();
                    }

                    return GetToken();
                }
                finally
                {
                    _lock.Release(1);
                }
            }

            return await GetTokenAsync();
        }

        private async Task AuthenticateAsync()
        {
            _logger.LogTrace("Starting the authentication code flow to log in.");
            var result = await _client.LoginAsync();
            if (result.IsError)
            {
                _logger.LogTrace("Error during login: {error} - {error_description}.", result.Error, result.ErrorDescription);
                _tokenData = new TokenData { Error = result.Error, ErrorDescription = result.ErrorDescription };
                throw new UnauthorizedAccessException(result.Error);
            }

            _logger.LogTrace("Login was successful for {user}", result.User.Identity.Name);
            _tokenData = new TokenData
            {
                AccessToken = result.AccessToken,
                AccessTokenExpiration = result.AccessTokenExpiration,
                AuthenticationTime = result.AuthenticationTime,
                RefreshToken = result.RefreshToken,
                IdentityToken = result.IdentityToken,
                Error = result.Error,
                ErrorDescription = result.ErrorDescription,
                User = result.User,
            };
        }

        private async Task<TokenData> RefreshTokenAsync()
        {
            var refreshToken = _tokenData.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogTrace("No refresh token given.");
                _tokenData = null;

                return null;
            }
            _logger.LogTrace("Using refresh token to get a new token.");
            var result = await _client.RefreshTokenAsync(refreshToken);
            if (result.IsError)
            {
                _tokenData = new TokenData { Error = result.Error, ErrorDescription = result.ErrorDescription };
                _logger.LogTrace("Token refresh returned error {error} - {error_description}.", result.Error, result.ErrorDescription);
            }
            else
            {
                _tokenData = new TokenData
                {

                    AccessToken = result.AccessToken,
                    AccessTokenExpiration = result.AccessTokenExpiration,
                    RefreshToken = result.RefreshToken,
                    IdentityToken = result.IdentityToken,
                    Error = result.Error,
                    ErrorDescription = result.ErrorDescription,
                    ExpiresIn = result.ExpiresIn,
                };
            }

            return _tokenData;
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public class TokenData
        {
            public int ExpiresIn { get; internal set; }
            public string AccessToken { get; internal set; }
            public string RefreshToken { get; internal set; }
            public string IdentityToken { get; internal set; }
            public string Error { get; internal set; }
            public string ErrorDescription { get; internal set; }
            public DateTimeOffset AccessTokenExpiration { get; internal set; }
            public DateTimeOffset? AuthenticationTime { get; internal set; }
            public ClaimsPrincipal User { get; internal set; }
        }

        /// <summary>
        /// Sends a ping request to the authorization service and returns the response. A good response is usually "Healthy".
        /// </summary>
        public async ValueTask<string> PingAsync()
        {
            using (var client = new HttpClient(_handler, false))
            {
                var uriBuilder = new UriBuilder(_authority);
                uriBuilder.Path += "health";
                using (var response = await client.GetAsync(uriBuilder.Uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    return response.ReasonPhrase;
                }
            }
        }

        /// <summary>
        /// Disposes the inner handler in case it was not injected.
        /// </summary>
        public void Dispose()
        {
            if (_handlerWasCreated)
            {
                _lock.Dispose();
                _handler.Dispose();
            }
        }

        public ValueTask<AuthenticationResult> GetAuthenticationResultAsync()
        {
            var t = new AuthenticationResult
            {
                AccessToken = _tokenData.AccessToken,
                RefreshToken = _tokenData.RefreshToken,
                IdToken = _tokenData.IdentityToken,
                Name = _tokenData.User.Identity.Name, // this can be tricky: you have to differentiate between the username (e.g. localhost\someguy) and the display name (Some Guy)
                Username = _tokenData.User.Claims.FirstOrDefault(x => x.Type == "username")?.Value
            };
            return new ValueTask<AuthenticationResult>(t);
        }
    }
}
