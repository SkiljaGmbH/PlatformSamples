using NUnit.Framework;
using NUnit.Framework.Legacy;
using SampleEventDrivenActivity.Configuration;
using SampleEventDrivenActivity.Tests.Properties;
using STG.Common.DTO;
using STG.RT.API.Document.Factories;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SampleEventDrivenActivity.Tests
{
    [TestFixture]
    public class EventDrivenInitializerTests
    {
        private EventDrivenInitializer _sut;
        public const string MyDocTypeName = "myDocType";

        [SetUp]
        public void SetUp()
        {
            _sut = new EventDrivenInitializer();
            _sut.DocumentApiFactory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true, DocumentTypes = new List<DtoDocumentTypeDefinition> { GetMyDocumentType() } });
            _sut.ActivityConfiguration = new EventDrivenInitializerSettings();
        }

        [Test]
        public void FillDocumentFromZipStructure_CreatePages_PagesAreCreatedForMedia()
        {
            _sut.ActivityConfiguration.CreatePages = true;
            var stgdoc = _sut.DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            _sut.FillDocumentFromZipStructure(stgdoc, new MemoryStream(Resources.StreamInputOnlyRoot));

            ClassicAssert.AreEqual(3, stgdoc.Pages.Count, "Should be 3 media and each of it should be assigned to a page.");
        }

        [Test]
        public void FillDocumentFromZipStructure_RootWithMetaData_DocumentInitialized()
        {
            var stgdoc = _sut.DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            _sut.FillDocumentFromZipStructure(stgdoc, new MemoryStream(Resources.StreamInput_StructureAndMetaData));

            Assert.That(stgdoc.Name, Is.EqualTo("name of my root doc"), "Value is given via metadata");
            Assert.That(stgdoc.DocumentType, Is.EqualTo(MyDocTypeName));
            Assert.That(stgdoc.IndexFields.Count, Is.EqualTo(2));
            Assert.That(stgdoc.IndexFields.Select(x => x.FieldName), Contains.Item("field1"));
            Assert.That(stgdoc.IndexFields.Select(x => x.FieldValue.GetText()), Contains.Item("val1"));

            Assert.That(stgdoc.Tables[0].Rows.Count, Is.EqualTo(2));
            Assert.That(stgdoc.Tables[0].Rows[0].Cells[0].CellValue.GetText(), Is.EqualTo("colValue1"));
            Assert.That(stgdoc.Tables[0].Rows[0].Cells[1].CellValue.GetText(), Is.EqualTo("colValue2"));
        }

        public class TestEntryHelper : IEntryHelper
        {
            public TestEntryHelper(string fullName)
            {
                var name = Path.GetFileName(fullName);
                DirectoryName = Path.GetDirectoryName(fullName);
                FileName = Path.GetFileName(name);
                Extension = Path.GetExtension(name);
            }

            public string Extension { get; set; }
            public string ExtensionWithoutDot => Extension?.Substring(1) ?? "";
            public string FileName { get; set; }
            public string DirectoryName { get; set; }
            public MemoryStream Stream { get; set; }
            public Stream Open()
            {
                return Stream;
            }
        }

        [Test]
        public void GetSubLevels_TwoLevelTwos_TwoLists()
        {
            var entryHelpers = new List<IEntryHelper>
            {
                new TestEntryHelper("metadata.json"),
                new TestEntryHelper("lvl1_a\\media1.png"),
                new TestEntryHelper("lvl1_a\\metadata.json"),
                new TestEntryHelper("lvl1_b\\media1.png"),
                new TestEntryHelper("lvl1_b\\metadata.json"),
            };

            var res = _sut.GroupSubDirectories(entryHelpers, "");
            Assert.That(res.Count, Is.EqualTo(2));
            Assert.That(res.SelectMany(x => x).Count(), Is.EqualTo(4));
        }

        [Test]
        public void GetSubLevels_ThirdLevel_OneList()
        {
            var entryHelpers = new List<IEntryHelper>
            {
                new TestEntryHelper("metadata.json"),
                new TestEntryHelper("lvl1_a\\media1.png"),
                new TestEntryHelper("lvl1_a\\metadata.json"),
                new TestEntryHelper("lvl1_a\\thirdLvl\\metadata.json"),
                new TestEntryHelper("lvl1_a\\thirdLvl\\media1.png"),
            };

            var res = _sut.GroupSubDirectories(entryHelpers, "lvl1_a");
            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res.SelectMany(x => x).Count(), Is.EqualTo(2));
        }

        [Test]
        public void FillDocumentFromZipStructure_FullStructure_ChildrenInitialized()
        {
            DtoWorkItemData workItem = new DtoWorkItemData() { WorkItemID = 13 };
            var rootDoc = _sut.DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(workItem);
            _sut.FillDocumentFromZipStructure(rootDoc, new MemoryStream(Resources.StreamInput_StructureAndMetaData));

            Assert.That(workItem.DocumentID, Is.EqualTo(rootDoc.ID));
            Assert.That(rootDoc.ChildDocuments.Count, Is.EqualTo(2));
            Assert.That(rootDoc.ChildDocuments.Select(x => x.Name), Is.EquivalentTo(new[] { "lvl1_1", "lvl1_2" }));
            Assert.That(rootDoc.ChildDocuments[0].ChildDocuments.Count, Is.EqualTo(1));
            Assert.That(rootDoc.ChildDocuments[0].ChildDocuments[0].Name, Is.EqualTo("lvl2_filled"));
        }

        [Test]
        public void CreateMedia_UnknownMedia_CreatesUnknown()
        {
            _sut.ActivityConfiguration.HandleUnknownExtensions = UnknownMediaExtensionHandling.ImportAsUnknown;
            var media = _sut.CreateMedia(new TestEntryHelper("media.ppng") { Stream = new MemoryStream(new byte[10]) });
            Assert.That(media, Is.Not.Null);
        }

        [Test]
        public void ApplyMetaData_CustomValues_Applied()
        {
            var m = new MetaData();
            m.CustomValues.Add(new CustomValue { Key = "todo", Value = "some value" });
            var doc = _sut.DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(new DtoWorkItemData());
            _sut.ApplyMetaData(doc, m);
            Assert.That(doc.CustomValues[0].Key, Is.EqualTo("todo"));
            Assert.That(doc.CustomValues[0].Value, Is.EqualTo("some value"));
        }

        /// <summary>
        /// Gets the document type that is used by the metadata.json's in the test documents
        /// </summary>
        public static DtoDocumentTypeDefinition GetMyDocumentType()
        {
            var docType = new DtoDocumentTypeDefinition();
            docType.Name = MyDocTypeName;
            docType.FieldDefinitions.Add(new DtoStorageDefinition { Name = "field1", FieldType = DtoSTGDataType.STGString });
            docType.FieldDefinitions.Add(new DtoStorageDefinition { Name = "field2", FieldType = DtoSTGDataType.STGString });
            docType.TableDefinitions.Add(new DtoTableDefinition()
            {
                Name = "myTable",
                ColumnDefinitions = new List<DtoStorageDefinition>
            {
                new DtoStorageDefinition{Name = "col1", FieldType = DtoSTGDataType.STGString},
                new DtoStorageDefinition{Name = "col2", FieldType = DtoSTGDataType.STGString},
            }
            });
            return docType;
        }
    }
}
