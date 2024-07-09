using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using STG.Common.Utilities.Logging;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Platform version checker
    /// </summary>
    public static class PlatformVersionChecker
    {
        private class VersionCacheEntry
        {
            public DateTime DateTime { get; set; }
            public Version Version { get; set; }
        }

        private class ServiceVersion30
        {
            public string ProductVersionInfo { get; set; }
            public string ExactVersion { get; set; }
        }

        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Only use 1 handler!!!
        /// </summary>
        private static readonly HttpClientHandler _httpClientHandler = new HttpClientHandler();
        private static readonly Regex _versionRegex = new Regex("[.]{0,3}\\d+\\.\\d+", RegexOptions.Compiled);
        private static readonly Regex _servicePackRegex = new Regex($"(ServicePack )(\\d+)", RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<string, VersionCacheEntry> _platformVersions = new ConcurrentDictionary<string, VersionCacheEntry>();

        public static async Task<Version> GetPlatformVersionAsync(string configurationServiceEndpoint)
        {
            if (!configurationServiceEndpoint.EndsWith("/")) configurationServiceEndpoint += "/";

            var version = GetPlatformVersionFromCache(configurationServiceEndpoint);

            if (version == null)
            {
                version = await GetPlatformVersionFromServiceVersionEndpointAsync(configurationServiceEndpoint);
            }

            if (version == null)
            {
                version = await GetPlatformVersionFromVersionEndpointAsync(configurationServiceEndpoint);
            }

            return version;
        }

        private static Version GetPlatformVersionFromCache(string configurationServiceEndpoint)
        {
            if (_platformVersions.TryGetValue(configurationServiceEndpoint, out var versionCacheEntry))
            {
                if (versionCacheEntry.DateTime < DateTime.UtcNow.AddHours(-1))
                {
                    _platformVersions.TryRemove(configurationServiceEndpoint, out _);
                }
                else
                {
                    // cache hit
                    return versionCacheEntry.Version;
                }
            }

            return null;
        }

        private static async Task<Version> GetPlatformVersionFromServiceVersionEndpointAsync(string configurationServiceEndpoint)
        {
            try
            {
                using (var httpClient = new HttpClient(_httpClientHandler, false))
                {
                    var uriBuilder = new UriBuilder(configurationServiceEndpoint);

                    // this url exists since at least 3.0
                    uriBuilder.Path += "api/configservice/serviceversion";

                    var httpResponseMessage = await httpClient.GetAsync(uriBuilder.Uri);
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var content = await httpResponseMessage.Content.ReadAsStringAsync();
                        var serviceVersion30 = JsonConvert.DeserializeObject<ServiceVersion30>(content);

                        if (serviceVersion30.ExactVersion != null && Version.TryParse(serviceVersion30.ExactVersion, out var version))
                        {
                            _platformVersions.TryAdd(configurationServiceEndpoint, new VersionCacheEntry() { Version = version, DateTime = DateTime.UtcNow });
                            _logger.Info($"The endpoint {configurationServiceEndpoint} has version {version}.");
                            return version;
                        }
                        else
                        {
                            _logger.Info($"The endpoint {configurationServiceEndpoint} does not return an expected response (gave: {content}).");
                        }
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.Info($"The endpoint {configurationServiceEndpoint} does not support the serviceversion endpoint (not found) and is thus either wrong or less than version 3.0.");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.InfoException("Failure to obtain information about the endpoint version", e);
            }

            return null;
        }

        private static async Task<Version> GetPlatformVersionFromVersionEndpointAsync(string configurationServiceEndpoint)
        {
            try
            {
                using (var httpClient = new HttpClient(_httpClientHandler, false))
                {
                    var uriBuilder = new UriBuilder(configurationServiceEndpoint);

                    // this url exists since at least 2.4 sp1
                    uriBuilder.Path += "api/configservice/version";

                    var httpResponseMessage = await httpClient.GetAsync(uriBuilder.Uri);
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var content = await httpResponseMessage.Content.ReadAsStringAsync();
                        var versionRegexMatch = _versionRegex.Match(content);
                        if (versionRegexMatch.Success && Version.TryParse(versionRegexMatch.Groups[0].Value, out var version))
                        {
                            var servicePackRegexMatch = _servicePackRegex.Match(content);
                            if (servicePackRegexMatch.Success && int.TryParse(servicePackRegexMatch.Groups[2].Value, out var servicePack))
                            {
                                version = new Version(version.Major, version.Minor, servicePack);
                            }
                            _platformVersions.TryAdd(configurationServiceEndpoint, new VersionCacheEntry() { Version = version, DateTime = DateTime.UtcNow });
                            _logger.Info($"The endpoint {configurationServiceEndpoint} has version {version}.");
                            return version;
                        }
                        else
                        {
                            _logger.Info($"The endpoint {configurationServiceEndpoint} does not return an expected response (gave: {content}).");
                        }
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.Info($"The endpoint {configurationServiceEndpoint} does not support the version endpoint (not found) and is thus either wrong or less than version 2.4.");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.InfoException("Failure to obtain information about the endpoint version", e);
            }

            return null;
        }
    }
}
