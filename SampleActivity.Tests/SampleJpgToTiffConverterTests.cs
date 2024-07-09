using STG.Common.DTO;
using STG.RT.API.Document.Factories;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using STG.RT.API.Document;

namespace SampleActivity.Tests
{
    [TestFixture]
    public class SampleJpgToTiffConverterTests
    {
        [Test]
        public void SampleJpgToTiffConverter_FunctionalTest()
        {
            // Create activity
            SampleJpgToTiffConverter activity = new SampleJpgToTiffConverter();

            // Create document API factory for local work (without attempting to access the database)
            var docApiFactory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });
            activity.DocumentApiFactory = docApiFactory;

            // Set activity configuration if necessary
            // activity.ActivityConfiguration.RoutingCustomValueName = "some name";

            var wi = new DtoWorkItemData();
            // Create STGDocument with a JPEG media
            var stgDoc = docApiFactory.CreateDocumentFactory().CreateSTGDoc(wi);
            DtoMediaType jpegMediaType = docApiFactory.CreateMediaTypeService().LoadAllMediaTypes().First(m => m.MediaTypeName.Equals("jpg", StringComparison.OrdinalIgnoreCase));
            var jpegFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Test.jpg");
            STGMedia jpgMedia = STGMedia.Initialize("jpegMedia", jpegMediaType, jpegFile, true);
            stgDoc.AppendMedia(jpgMedia);
            var page = stgDoc.AppendPage(new STGPage());
            page.AppendMedia(jpgMedia, new STGImageBasedPageLocation(jpgMedia.ID, 0), false);

            // Run test
            Assert.That(stgDoc.Media.Count(m => m.MediaType.MediaTypeName.Equals("tiff", StringComparison.OrdinalIgnoreCase)) == 0, Is.True);
            activity.Process(wi, stgDoc);
            Assert.That(stgDoc.Media.Count(m => m.MediaType.MediaTypeName.Equals("tiff", StringComparison.OrdinalIgnoreCase)) > 0, Is.True);
        }
    }
}
