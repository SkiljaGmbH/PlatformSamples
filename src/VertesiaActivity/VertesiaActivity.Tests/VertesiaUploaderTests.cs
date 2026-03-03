using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using STG.Common.DTO;
using STG.RT.API.Document;
using STG.RT.API.Document.Factories;
using VertesiaActivity.Settings;

namespace VertesiaActivity.Tests
{
    [TestFixture]
    public class VertesiaUploaderSettingsTests
    {
        [Test]
        public void DefaultConstructor_InitializesResultMapping()
        {
            var settings = new VertesiaUploaderSettings();

            Assert.That(settings.ResultMapping, Is.Not.Null);
            Assert.That(settings.ResultMapping.Count, Is.EqualTo(0));
        }

        [Test]
        public void Properties_CanBeSetAndRead()
        {
            var settings = new VertesiaUploaderSettings
            {
                ApiKey = "my-secret-key",
                ApiUrl = "https://api.vertesia.io",
                ContentType = "my_document_type",
                InteractionId = "interaction-abc"
            };
            settings.ResultMapping["field1"] = "cv_field1";

            Assert.That(settings.ApiKey, Is.EqualTo("my-secret-key"));
            Assert.That(settings.ApiUrl, Is.EqualTo("https://api.vertesia.io"));
            Assert.That(settings.ContentType, Is.EqualTo("my_document_type"));
            Assert.That(settings.InteractionId, Is.EqualTo("interaction-abc"));
            Assert.That(settings.ResultMapping["field1"], Is.EqualTo("cv_field1"));
        }
    }

    [TestFixture]
    public class VertesiaUploaderApplyResultMappingTests
    {
        private static DocumentApiFactory CreateLocalFactory() =>
            new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });

        private static STGDocument CreateDocument()
        {
            var factory = CreateLocalFactory();
            return factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
        }

        [Test]
        public void ApplyResultMapping_MapsMatchingFields()
        {
            var activity = new VertesiaUploader();
            var doc = CreateDocument();

            var results = new Dictionary<string, string>
            {
                ["invoice_number"] = "INV-001",
                ["vendor_name"] = "ACME Corp"
            };
            var mapping = new SerializableDictionary<string, string>
            {
                ["invoice_number"] = "cv_InvoiceNumber",
                ["vendor_name"] = "cv_VendorName"
            };

            activity.ApplyResultMapping(doc, results, mapping);

            Assert.That(doc.LoadCustomValue("cv_InvoiceNumber"), Is.EqualTo("INV-001"));
            Assert.That(doc.LoadCustomValue("cv_VendorName"), Is.EqualTo("ACME Corp"));
        }

        [Test]
        public void ApplyResultMapping_IgnoresMissingResultFields()
        {
            var activity = new VertesiaUploader();
            var doc = CreateDocument();

            var results = new Dictionary<string, string>
            {
                ["present_field"] = "value"
            };
            var mapping = new SerializableDictionary<string, string>
            {
                ["present_field"] = "cv_Present",
                ["missing_field"] = "cv_Missing"
            };

            // Should not throw; missing_field is simply not set.
            Assert.DoesNotThrow(() => activity.ApplyResultMapping(doc, results, mapping));
            Assert.That(doc.LoadCustomValue("cv_Present"), Is.EqualTo("value"));
        }

        [Test]
        public void ApplyResultMapping_NullMapping_DoesNotThrow()
        {
            var activity = new VertesiaUploader();
            var doc = CreateDocument();
            var results = new Dictionary<string, string> { ["x"] = "y" };

            Assert.DoesNotThrow(() => activity.ApplyResultMapping(doc, results, null));
        }

        [Test]
        public void ApplyResultMapping_NullResults_DoesNotThrow()
        {
            var activity = new VertesiaUploader();
            var doc = CreateDocument();
            var mapping = new SerializableDictionary<string, string> { ["x"] = "cv_x" };

            Assert.DoesNotThrow(() => activity.ApplyResultMapping(doc, null, mapping));
        }

        [Test]
        public void ApplyResultMapping_IsCaseInsensitiveOnResultKeys()
        {
            var activity = new VertesiaUploader();
            var doc = CreateDocument();

            var results = new Dictionary<string, string>
            {
                // Returned by the API in mixed case
                ["InvoiceNumber"] = "INV-999"
            };
            var mapping = new SerializableDictionary<string, string>
            {
                // Configured in lowercase in the mapping
                ["invoicenumber"] = "cv_InvoiceNumber"
            };

            activity.ApplyResultMapping(doc, results, mapping);

            Assert.That(doc.LoadCustomValue("cv_InvoiceNumber"), Is.EqualTo("INV-999"));
        }
    }

    [TestFixture]
    public class VertesiaUploaderProcessTests
    {
        private static DocumentApiFactory CreateLocalFactory() =>
            new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });

        [Test]
        public void Process_DocumentWithNoChildren_MakesNoHttpCalls()
        {
            var handler = new MockHttpMessageHandler(); // no responses queued
            var httpClient = new HttpClient(handler);
            var activity = new VertesiaUploader(httpClient);
            ConfigureActivity(activity);

            var factory = CreateLocalFactory();
            var doc = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());

            // A document with no children must complete without attempting any HTTP calls.
            Assert.DoesNotThrow(() => activity.Process(new DtoWorkItemData(), doc));
            Assert.That(handler.RemainingResponses, Is.EqualTo(0));
        }

        [Test]
        public void Process_FullHappyPath_SetsCustomValuesOnChildDocument()
        {
            var handler = new MockHttpMessageHandler();

            // 1. JWT token
            handler.Enqueue(JsonResponse("\"test-jwt-token\""));
            // 2. Upload URL
            handler.Enqueue(JsonResponse(JsonSerializer.Serialize(new
            {
                url = "https://storage.example.com/upload-target",
                id = "gs://bucket/object-abc",
                mime_type = "application/pdf",
                path = "bucket/object-abc"
            })));
            // 3. PUT upload (no body needed)
            handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
            // 4. Register object
            handler.Enqueue(JsonResponse(JsonSerializer.Serialize(new { id = "object-123" })));
            // 5. Status poll → ready immediately
            handler.Enqueue(JsonResponse(JsonSerializer.Serialize(new { status = "ready" })));
            // 6. Execute interaction
            handler.Enqueue(JsonResponse(JsonSerializer.Serialize(new
            {
                Results = new { invoice_number = "INV-001", total_amount = "500.00" }
            })));

            var httpClient = new HttpClient(handler);
            var activity = new VertesiaUploader(httpClient);
            ConfigureActivity(activity);
            activity.ActivityConfiguration.ResultMapping = new SerializableDictionary<string, string>
            {
                ["invoice_number"] = "cv_InvoiceNumber",
                ["total_amount"] = "cv_TotalAmount"
            };

            var factory = CreateLocalFactory();
            var workItem = new DtoWorkItemData();
            var parentDoc = factory.CreateDocumentFactory().CreateSTGDoc(workItem);
            var childDoc = factory.CreateDocumentFactory().CreateSTGDoc(workItem);

            // Attach a PDF media file to the child document.
            var mediaTypes = factory.CreateMediaTypeService().LoadAllMediaTypes();
            var pdfMediaType = System.Linq.Enumerable.FirstOrDefault(
                mediaTypes,
                m => string.Equals(m.MediaTypeName, "pdf", System.StringComparison.OrdinalIgnoreCase));

            // Skip if the local factory doesn't expose a PDF media type.
            Assume.That(pdfMediaType, Is.Not.Null, "PDF media type not available in local factory; skipping.");

            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, Encoding.UTF8.GetBytes("%PDF-1.4 fake content"));
                var media = STGMedia.Initialize("test.pdf", pdfMediaType, tempFile, true);
                childDoc.AppendMedia(media);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            parentDoc.AppendChild(childDoc);

            activity.Process(workItem, parentDoc);

            Assert.That(handler.RemainingResponses, Is.EqualTo(0), "All mocked HTTP responses should have been consumed.");
            Assert.That(childDoc.LoadCustomValue("cv_InvoiceNumber"), Is.EqualTo("INV-001"));
            Assert.That(childDoc.LoadCustomValue("cv_TotalAmount"), Is.EqualTo("500.00"));
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static void ConfigureActivity(VertesiaUploader activity)
        {
            activity.ActivityConfiguration.ApiKey = "test-api-key";
            activity.ActivityConfiguration.ApiUrl = "https://api.vertesia.io";
            activity.ActivityConfiguration.ContentType = "my_doc_type";
            activity.ActivityConfiguration.InteractionId = "interaction-xyz";
        }

        private static HttpResponseMessage JsonResponse(string json) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
    }
}
