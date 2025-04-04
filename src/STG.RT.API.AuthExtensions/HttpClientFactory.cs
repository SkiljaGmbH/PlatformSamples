using System;
using System.Net.Http;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Creates HttpClients and keeps the one static HttpClientHandler that the application should use (unless the factory is torn down)
    /// </summary>
    public class HttpClientFactory
    {
        private static Lazy<HttpClientFactory> Instance { get; set; } = new Lazy<HttpClientFactory>(() => new HttpClientFactory());

        private HttpClientHandler HttpClientHandler { get; } = new HttpClientHandler { UseDefaultCredentials = true };

        public static HttpClientHandler GetHandler()
        {
            return Instance.Value.HttpClientHandler;
        }

        public static HttpClient CreateHttpClient()
        {
            return Instance.Value.Create();
        }

        private HttpClient Create()
        {
            return new HttpClient(HttpClientHandler, false);
        }

        /// <summary>
        /// Tears down the http client factory and resets all things that were done
        /// </summary>
        public static void Teardown()
        {
            var lazy = Instance;
            Instance = new Lazy<HttpClientFactory>(() => new HttpClientFactory());
            if (lazy.IsValueCreated)
            {
                lazy.Value.HttpClientHandler.Dispose();
            }
        }
    }
}
