using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace EDAClient.ViewModels
{
    public class SslCertificateMonitor
    {
        private readonly Uri _uri;

        public SslCertificateMonitor(Uri uri)
        {
            _uri = uri;
        }

        private static readonly Dictionary<string, Uri> _uris = new Dictionary<string, Uri>();

        public static SslCertificateMonitor HookIn(Uri uri)
        {
            var me = new SslCertificateMonitor(uri);
            me.HookIn();
            return me;
        }

        private void HookIn()
        {
            ServicePointManager.ServerCertificateValidationCallback += LogErrorOnInvalidCertificate;
        }

        public void HookOut()
        {
            ServicePointManager.ServerCertificateValidationCallback -= LogErrorOnInvalidCertificate;
        }

        private bool LogErrorOnInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool validCert = sslPolicyErrors == SslPolicyErrors.None;
            if (!validCert)
            {
                var expirationDate = certificate.GetExpirationDateString();
                DateTime certificateExpirationDate;
                var endpoint = TryGetEndPoint(sender);
                if (DateTime.TryParse(expirationDate, out certificateExpirationDate) && certificateExpirationDate < DateTime.UtcNow)
                {
                    ErrorMessage = string.Format("The remote certificate has expired. {0}",
                        new { certificateExpirationDate, endpoint, certificate, sslPolicyErrors });
                }
                else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && chain.ChainStatus.Any(x => x.Status.HasFlag(X509ChainStatusFlags.UntrustedRoot)))
                {
                    ErrorMessage = string.Format("The remote certificate was issued by an untrusted root certification authority. {0}",
                        new { endpoint, certificate, sslPolicyErrors });
                }
                else
                {
                    ErrorMessage = string.Format("The remote certificate is invalid. {0}",
                        new { endpoint, certificate, sslPolicyErrors });
                }
            }

            return validCert;
        }

        public bool IsSslError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }

        private static string TryGetEndPoint(object sender)
        {
            var request = sender as HttpWebRequest;
            if (request != null)
            {
                return request.Address.Authority;
            }
            return "<unknown>";
        }


    }
}