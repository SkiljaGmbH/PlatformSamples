using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using VertesiaActivity.Settings;

namespace VertesiaActivity
{
    /// <summary>
    /// Uploads each child document to Vertesia, waits for processing to complete,
    /// executes a configured interaction, and maps the results back to the child
    /// document as custom values.
    ///
    /// Flow per child document:
    ///   1. POST https://sts.vertesia.io/token/issue  (ApiKey → JWT)
    ///   2. POST {ApiUrl}/objects/upload-url          (→ pre-signed URL + object id)
    ///   3. PUT  {pre-signed URL}                     (upload binary)
    ///   4. POST {ApiUrl}/objects                     (register object)
    ///   5. GET  {ApiUrl}/objects/{object_id}         (poll until status == "ready")
    ///   6. POST {ApiUrl}/interactions/{InteractionId}/execute  (→ Results JSON)
    ///   7. Map Results fields → child document custom values via ResultMapping
    /// </summary>
    public class VertesiaUploader : STGUnattendedAbstract<VertesiaUploaderSettings>
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private const string TokenIssueUrl = "https://sts.vertesia.io/token/issue";
        private const string ReadyStatus = "ready";
        private const int PollIntervalMs = 5000;
        private const int MaxPollAttempts = 60;

        private readonly HttpClient _httpClient;

        // Called by the STG framework via reflection.
        public VertesiaUploader() : this(new HttpClient()) { }

        // Called by unit tests to inject a mock handler.
        internal VertesiaUploader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            foreach (var childDocument in document.ChildDocuments)
            {
                ProcessChildDocument(childDocument);
            }
        }

        // -------------------------------------------------------------------------
        // Private pipeline
        // -------------------------------------------------------------------------

        private void ProcessChildDocument(STGDocument childDocument)
        {
            var media = childDocument.Media?.Count > 0 ? childDocument.Media[0] : null;
            if (media == null)
            {
                Log.Warn($"Child document {childDocument.ID} has no media; skipping.");
                return;
            }

            var jwt = GetJwtToken(ActivityConfiguration.ApiKey);

            var uploadInfo = GetUploadUrl(jwt, ActivityConfiguration.ApiUrl, ActivityConfiguration.ContentType);
            Log.Debug($"Got upload URL for object id: {uploadInfo.Id}");

            if (media.MediaStream.CanSeek)
                media.MediaStream.Seek(0, SeekOrigin.Begin);

            UploadDocument(uploadInfo.Url, media.MediaStream, ActivityConfiguration.ContentType);
            Log.Debug($"Uploaded child document {childDocument.ID}.");

            var fileName = $"{childDocument.ID}_{media.ID}";
            var mimeType = media.MediaType?.MediaTypeName ?? ActivityConfiguration.ContentType;
            var registerResponse = RegisterObject(
                jwt,
                ActivityConfiguration.ApiUrl,
                ActivityConfiguration.ContentType,
                uploadInfo.Id,
                fileName,
                mimeType);

            var objectId = registerResponse?.Id;
            Log.Debug($"Registered object id: {objectId}");

            WaitForObjectReady(jwt, ActivityConfiguration.ApiUrl, objectId);
            Log.Debug($"Object {objectId} is ready.");

            var results = ExecuteInteraction(jwt, ActivityConfiguration.ApiUrl, ActivityConfiguration.InteractionId);
            Log.Debug($"Interaction returned {results.Count} result field(s).");

            ApplyResultMapping(childDocument, results, ActivityConfiguration.ResultMapping);
        }

        // -------------------------------------------------------------------------
        // Step 1 – Authenticate
        // -------------------------------------------------------------------------

        private string GetJwtToken(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenIssueUrl);
            request.Headers.Add("Authorization", apiKey);

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // Response is a plain JSON string: "\"<token>\""
            return body.Trim().Trim('"');
        }

        // -------------------------------------------------------------------------
        // Step 2 – Request pre-signed upload URL
        // -------------------------------------------------------------------------

        private UploadUrlResponse GetUploadUrl(string jwt, string apiUrl, string contentType)
        {
            var url = apiUrl.TrimEnd('/') + "/objects/upload-url";
            var body = JsonSerializer.Serialize(new { content_type = contentType });

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<UploadUrlResponse>(responseBody, JsonOptions);
        }

        // -------------------------------------------------------------------------
        // Step 3 – Upload binary to pre-signed URL
        // -------------------------------------------------------------------------

        private void UploadDocument(string uploadUrl, Stream documentStream, string contentType)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Content = new StreamContent(documentStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        // -------------------------------------------------------------------------
        // Step 4 – Register the object in Vertesia
        // -------------------------------------------------------------------------

        private RegisterObjectResponse RegisterObject(
            string jwt,
            string apiUrl,
            string contentType,
            string sourceId,
            string fileName,
            string mimeType)
        {
            var url = apiUrl.TrimEnd('/') + "/objects";
            var body = JsonSerializer.Serialize(new
            {
                type = contentType,
                content = new
                {
                    source = sourceId,
                    name = fileName,
                    type = mimeType
                },
                properties = new { }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<RegisterObjectResponse>(responseBody, JsonOptions);
        }

        // -------------------------------------------------------------------------
        // Step 5 – Poll until status == "ready"
        // -------------------------------------------------------------------------

        private void WaitForObjectReady(string jwt, string apiUrl, string objectId)
        {
            var url = apiUrl.TrimEnd('/') + "/objects/" + objectId;

            for (var attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var status = JsonSerializer.Deserialize<ObjectStatusResponse>(body, JsonOptions);

                if (string.Equals(status?.Status, ReadyStatus, StringComparison.OrdinalIgnoreCase))
                    return;

                Log.Debug($"Object {objectId} status '{status?.Status}' (attempt {attempt}/{MaxPollAttempts}).");

                if (attempt < MaxPollAttempts)
                    Thread.Sleep(PollIntervalMs);
            }

            throw new InvalidOperationException(
                $"Object '{objectId}' did not reach '{ReadyStatus}' status after {MaxPollAttempts} attempts.");
        }

        // -------------------------------------------------------------------------
        // Step 6 – Execute interaction
        // -------------------------------------------------------------------------

        private Dictionary<string, string> ExecuteInteraction(string jwt, string apiUrl, string interactionId)
        {
            var url = apiUrl.TrimEnd('/') + "/interactions/" + interactionId + "/execute";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var doc = JsonDocument.Parse(body))
            {
                if (doc.RootElement.TryGetProperty("Results", out var resultsElement)
                    && resultsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in resultsElement.EnumerateObject())
                    {
                        results[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString()
                            : prop.Value.ToString();
                    }
                }
            }

            return results;
        }

        // -------------------------------------------------------------------------
        // Step 7 – Map results to child document custom values
        // -------------------------------------------------------------------------

        internal void ApplyResultMapping(
            STGDocument childDocument,
            Dictionary<string, string> results,
            SerializableDictionary<string, string> mapping)
        {
            if (mapping == null || results == null)
                return;

            foreach (var entry in mapping)
            {
                if (results.TryGetValue(entry.Key, out var value))
                {
                    childDocument.AddCustomValue(entry.Value, value, true);
                    Log.Debug($"Mapped '{entry.Key}' → '{entry.Value}': {value}");
                }
                else
                {
                    Log.Warn($"Result field '{entry.Key}' not found; custom value '{entry.Value}' was not set.");
                }
            }
        }

        // -------------------------------------------------------------------------
        // Response DTOs
        // -------------------------------------------------------------------------

        private sealed class UploadUrlResponse
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("mime_type")]
            public string MimeType { get; set; }

            [JsonPropertyName("path")]
            public string Path { get; set; }
        }

        private sealed class RegisterObjectResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        private sealed class ObjectStatusResponse
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
    }
}
