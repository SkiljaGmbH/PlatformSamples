using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Implements the <see cref="IBrowser"/> that handles the part of getting the authorization code from the authorize response.
    /// The code response is then used by <see cref="OidcAuthenticator"/> to obtain the access token via the token endpoint.
    /// </summary>
    public class Browser : IBrowser
    {
        private Uri _authority;

        /// <summary>
        /// Initializes a new instance of the <see cref="Browser"/>.
        /// </summary>
        /// <param name="authorityUri">The URI of the authorization service.</param>
        /// <exception cref="ArgumentNullException">If the authorityUri is not provided.</exception>
        public Browser(Uri authorityUri)
        {
            _authority = authorityUri ?? throw new ArgumentNullException(nameof(authorityUri));
        }

        /// <inheritdoc />
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            using (var http = new HttpListener())
            {
                http.Prefixes.Add(options.EndUrl);
                http.Start();

                // open system browser to start authentication
                var url = options.StartUrl.Replace("&", "^&");
                System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

                // wait for the authorization response.
                var context = await http.GetContextAsync();
                var formData = GetRequestPostData(context.Request) ?? context.Request.Url.ToString();

                // sends an HTTP response to the browser.
                var response = context.Response;
                string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url={0}'></head><body>Please return to the app.</body></html>", _authority);
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                responseOutput.Close();

                return new BrowserResult { Response = formData, ResultType = BrowserResultType.Success };
            }
        }
        private static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }

}
