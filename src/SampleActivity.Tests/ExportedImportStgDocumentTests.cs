
using NUnit.Framework;
using STG.Common.DTO;
using STG.RT.API.Document;
using STG.RT.API.Document.Conversion;
using STG.RT.API.Document.Factories;
using System;
using System.IO;

namespace SampleActivity.Tests;

[TestFixture]
public class ExportedImportStgDocumentTests
{
    [Test]
    public void CreateExportAndReimportStgdoc()
    {
        // This unit test shows how real exported data can be used in unit tests to ensure behavior of more complex activities.
        var docApiFactory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });
        var wi = new DtoWorkItemData();
        var stgDoc = docApiFactory.CreateDocumentFactory().CreateSTGDoc(wi);
        stgDoc.AddCustomValue("secret", "value", true);

        // export the stgdocument to an .stgdoc file
        var stgDocPath = ExportStgDocumentToDisk(docApiFactory, stgDoc);

        // now reimport that .stgdoc (you can also use any document exported from the platform)
        var reimportedDoc = ImportStgDocumentFromDisk(stgDocPath, docApiFactory);

        Assert.That(reimportedDoc.ID, Is.Not.EqualTo(stgDoc.ID), "A reimported STGDocument must have a different GUID. An STGDocument lives in a system. After creating a new STGDocument from that data, a new STGDocument was created (hence, new guids). The same happens to all other IDs, like for pages, media and custom extensions.");
        Assert.That(reimportedDoc.LoadCustomValue("secret"), Is.EqualTo(stgDoc.LoadCustomValue("secret")), "Data within the document is preserved.");

        File.Delete(stgDocPath);
    }

    private static STGDocument ImportStgDocumentFromDisk(string stgDocPath, DocumentApiFactory docApiFactory)
    {
        using (var fs = File.OpenRead(stgDocPath))
        {
            var exportedDocData = DocumentApiFactory.CreateZipConverter().LoadDocumentZip(fs);
            var reimportedStgdoc = docApiFactory.CreateDocumentConverter().ConvertToSTGDocument(exportedDocData);
            return reimportedStgdoc;
        }
    }

    private static string ExportStgDocumentToDisk(DocumentApiFactory docApiFactory, STGDocument stgDoc)
    {
        var stgDocPath = $"my-doc-{Guid.NewGuid()}.stgdoc";
        using (var fs = File.OpenWrite(stgDocPath))
        {
            var exportedDocumentData = docApiFactory.CreateDocumentConverter()
                .ConvertToDtoDocumentPackage(stgDoc, new ExportPackageConversionOptions());
            Assert.That(exportedDocumentData.Document.ID, Is.EqualTo(stgDoc.ID), "the ID of the exported document data matches exactly what was given");
            DocumentApiFactory.CreateZipConverter().CreateDocumentZip(fs, exportedDocumentData);
        }

        return stgDocPath;
    }
}