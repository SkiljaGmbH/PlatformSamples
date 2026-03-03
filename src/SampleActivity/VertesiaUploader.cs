using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    public class VertesiaUploader : STGUnattendedAbstract<VertesiaUploaderSettings>
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private const string TokenIssueUrl = "https://sts.vertesia.io/token/issue";
        private const string ReadyStatus = "ready";
        private const int PollIntervalMs = 5000;
        private const int MaxPollAttempts = 60;

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            foreach (var childDocument in document.ChildDocuments)
            {
                ProcessChildDocument(childDocument);
            }
        }

        private void ProcessChildDocument(STGDocument childDocument)
        {
            var media = childDocument.Media?.Count > 0 ? childDocument.Media[0] : null;
            if (media == null)
            {
                Log.Warn($"Child document {childDocument.ID} has no media. Skipping.");
                return;
            }

            // Step 1: Authenticate with Vertesia STS and get JWT
            var jwtToken = GetJwtToken(ActivityConfiguration.ApiKey);

            // Step 2: Request a pre-signed upload URL
            var uploadUrlResponse = GetUploadUrl(jwtToken, ActivityConfiguration.ApiUrl, ActivityConfiguration.ContentType);
            Log.Debug($"Retrieved upload URL for object ID: {uploadUrlResponse.Id}");

            // Step 3: Upload the child document binary to the pre-signed URL
            if (media.MediaStream.CanSeek)
                media.MediaStream.Seek(0, SeekOrigin.Begin);

            UploadDocument(uploadUrlResponse.Url, media.MediaStream, ActivityConfiguration.ContentType);
            Log.Debug($"Uploaded child document {childDocument.ID} to object ID: {uploadUrlResponse.Id}");

            // Step 4: Register the uploaded object in Vertesia
            var fileName = $"{childDocument.ID}_{media.ID}";
            var mimeType = media.MediaType?.MediaTypeName ?? ActivityConfiguration.ContentType;
            var registerResponse = RegisterObject(jwtToken, ActivityConfiguration.ApiUrl, ActivityConfiguration.ContentType, uploadUrlResponse.Id, fileName, mimeType);
            var objectId = registerResponse?.Id;
            Log.Debug($"Registered object {uploadUrlResponse.Id} for child document {childDocument.ID}. Object ID: {objectId}");

            // Step 5: Poll until the object status is "ready"
            WaitForObjectReady(jwtToken, ActivityConfiguration.ApiUrl, objectId);
            Log.Debug($"Object {objectId} is ready.");
        }

        private string GetJwtToken(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenIssueUrl);
            request.Headers.Add("Authorization", apiKey);

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return content.Trim().Trim('"');
        }

        private UploadUrlResponse GetUploadUrl(string jwtToken, string apiUrl, string contentType)
        {
            var url = apiUrl.TrimEnd('/') + "/objects/upload-url";
            var body = JsonSerializer.Serialize(new { content_type = contentType });

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<UploadUrlResponse>(content, _jsonOptions);
        }

        private void UploadDocument(string uploadUrl, Stream documentStream, string contentType)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Content = new StreamContent(documentStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        private RegisterObjectResponse RegisterObject(string jwtToken, string apiUrl, string contentType, string sourceId, string fileName, string mimeType)
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<RegisterObjectResponse>(content, _jsonOptions);
        }

        private void WaitForObjectReady(string jwtToken, string apiUrl, string objectId)
        {
            var url = apiUrl.TrimEnd('/') + "/objects/" + objectId;

            for (var attempt = 1; attempt <= MaxPollAttempts; attempt++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var statusResponse = JsonSerializer.Deserialize<ObjectStatusResponse>(content, _jsonOptions);

                if (string.Equals(statusResponse?.Status, ReadyStatus, StringComparison.OrdinalIgnoreCase))
                    return;

                Log.Debug($"Object {objectId} status is '{statusResponse?.Status}'. Waiting... (attempt {attempt}/{MaxPollAttempts})");

                if (attempt < MaxPollAttempts)
                    Thread.Sleep(PollIntervalMs);
            }

            throw new InvalidOperationException($"Object {objectId} did not reach '{ReadyStatus}' status after {MaxPollAttempts} attempts.");
        }

        private class UploadUrlResponse
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

        private class RegisterObjectResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        private class ObjectStatusResponse
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
    }
}
