using Newtonsoft.Json;
using NUnit.Framework;
using SampleEventDrivenActivity.Configuration;
using STG.Common.DTO;
using STG.RT.API.Document;
using STG.RT.API.Document.Factories;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SampleEventDrivenActivity.Tests
{
    [TestFixture]
    public class EventDrivenNotifierTests
    {
        [Test]
        public void Process_SomeDocument_GetsJsonWithFieldValues()
        {
            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportFieldsAndTables = true;
            var factory = new DocumentApiFactory(new DocumentFactoryOptions
            {
                WorkLocally = true,
                DocumentTypes = new List<DtoDocumentTypeDefinition> { EventDrivenInitializerTests.GetMyDocumentType() }
            });
            var doc = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            doc.Initialize(EventDrivenInitializerTests.MyDocTypeName);
            doc.IndexFields[0].FieldValue.SetText("field text 1");
            doc.IndexFields[1].FieldValue.SetText("other test 2");

            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                sut.AddToArchive(archive, doc, "");
            }

            memoryStream.Position = 0;
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var e = archive.Entries.FirstOrDefault(x => x.Name == "metadata.json");
                using (var reader = new StreamReader(e.Open()))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var ser = new JsonSerializer();
                    var data = ser.Deserialize<ResponseMetaData>(jsonReader);
                    Assert.That(data.IndexFields[1].Value, Is.EqualTo("other test 2"));
                }
            }
        }

        [Test]
        public void Process_SomeDocument_ExportsMedia()
        {
            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportMedia = true;
            var factory = new DocumentApiFactory(new DocumentFactoryOptions
            {
                WorkLocally = true,
                DocumentTypes = new List<DtoDocumentTypeDefinition> { EventDrivenInitializerTests.GetMyDocumentType() }
            });
            var doc = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            doc.AppendMedia(new STGMediaBuilder().CreateMedia("myMedia", ".ext", new DtoMediaType { MediaTypeName = "pdf" }).WithStream(new MemoryStream(new byte[10])).ReleaseStreamOwnership().Finish());

            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                sut.AddToArchive(archive, doc, "");
            }

            memoryStream.Position = 0;
            using (var archive2 = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var e = archive2.Entries.FirstOrDefault(x => x.Name.StartsWith("myMedia"));
                Assert.That(e, Is.Not.Null);
            }
        }

        [Test]
        public void Process_WithExtensions_WritesExtensions()
        {
            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportExtensions = true;
            var factory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true, });

            var doc = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            doc.SetExtensionData(new Extension());

            var responseMetaData = new ResponseMetaData();
            sut.AddExtensions(responseMetaData, doc);
            Assert.That(responseMetaData.Extensions.Count, Is.EqualTo(1));
        }

        public class Extension : STGExtensionBase
        {
            public string SomeText { get; set; }
        }

        [Test]
        public void Process_ChildDoc_RecursivelyPutIntoZip()
        {
            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportFieldsAndTables = true;
            var factory = new DocumentApiFactory(new DocumentFactoryOptions
            {
                WorkLocally = true,
                DocumentTypes = new List<DtoDocumentTypeDefinition> { EventDrivenInitializerTests.GetMyDocumentType() }
            });
            var root = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            var doc = root.AppendChild();
            doc.Initialize(EventDrivenInitializerTests.MyDocTypeName);
            doc.IndexFields[0].FieldValue.SetText("field text 1");
            doc.IndexFields[1].FieldValue.SetText("other test 2");

            var stream = sut.Process(new DtoWorkItemData(), root);

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var e = archive.Entries.FirstOrDefault(x => x.FullName == doc.ID + "/metadata.json");
                using (var reader = new StreamReader(e.Open()))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var ser = new JsonSerializer();
                    var data = ser.Deserialize<ResponseMetaData>(jsonReader);
                    Assert.That(data.IndexFields[1].Value, Is.EqualTo("other test 2"));
                }
            }
        }

        [Test]
        public void CreateResponseMetaData_WithTables_AddsTables()
        {
            var factory = new DocumentApiFactory(new DocumentFactoryOptions
            {
                WorkLocally = true,
                DocumentTypes = new List<DtoDocumentTypeDefinition> { EventDrivenInitializerTests.GetMyDocumentType() }
            });
            var root = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            root.Initialize(EventDrivenInitializerTests.MyDocTypeName);
            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportFieldsAndTables = true;

            var expectedColumnName = root.Tables[0].ColumnDefinition[0].Name;
            var expectedRow = root.Tables[0].InsertNewRow(0);
            expectedRow.Cells[0].CellValue.SetText("cellText");

            var result = sut.CreateResponseMetaData(root);
            Assert.That(result.Tables.Count, Is.GreaterThan(0));
            Assert.That(result.Tables[0].Header[0], Is.EqualTo(expectedColumnName));
            Assert.That(result.Tables[0].Rows[0][0], Is.EqualTo(expectedRow.Cells[0].CellValue.GetText()));
        }

        [Test]
        public void CreateResponseMetaData_WithFields_AddsFields()
        {
            var factory = new DocumentApiFactory(new DocumentFactoryOptions
            {
                WorkLocally = true,
                DocumentTypes = new List<DtoDocumentTypeDefinition> { EventDrivenInitializerTests.GetMyDocumentType() }
            });
            var root = factory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            root.Initialize(EventDrivenInitializerTests.MyDocTypeName);

            var sut = new EventDrivenNotifier();
            sut.ActivityConfiguration = new EventDrivenNotifierSettings();
            sut.ActivityConfiguration.ExportFieldsAndTables = true;

            root.IndexFields[0].FieldValue.SetText("fieldVal");

            var result = sut.CreateResponseMetaData(root);
            Assert.That(result.IndexFields.Count, Is.EqualTo(root.IndexFields.Count));
            Assert.That(result.IndexFields[0].Name, Is.EqualTo(root.IndexFields[0].FieldName));
            Assert.That(result.IndexFields[0].Value, Is.EqualTo(root.IndexFields[0].FieldValue.GetText()));
        }
    }
}