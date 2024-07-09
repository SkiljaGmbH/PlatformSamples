using STG.Common.DTO;
using STG.RT.API.Document.Factories;
using NUnit.Framework;
using Moq;
using STG.RT.API.Document;
using STG.RT.API.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SampleActivity.Tests
{
    [TestFixture]
    public class SampleImportTimerTests
    {
        [Test]
        public void SampleImportTimerTests_FunctionalTest()
        {
            // Create activity
            var activity = new SampleImportTimer();

            // Create document API factory for local work (without attempting to access the database)
            var documentApiFactory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });
            activity.DocumentApiFactory = documentApiFactory;

            // Set activity configuration if necessary
            activity.ActivityConfiguration = new Settings.ImportSettings();
            activity.ActivityConfiguration.ImportPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            activity.ActivityConfiguration.TiffFilter = "Test.tiff";
            activity.ActivityConfiguration.JpegFilter = "Test.jpg";

            activity.ActivityInfo = new DtoActivityInstanceInfo { ActivityInstanceID = 1, Process = new DtoProcessInfo { ProcessID = 2 } };

            var createdDocs = new Dictionary<DtoWorkItemData, STGDocument>();
            var createdWis = new List<DtoWorkItemData>();

            var stgprocess = new Mock<ISTGProcess>();
            // Setup ISTGProcess mock to register work items/documents for further checks
            stgprocess.Setup(x => x.CreateWorkItem(It.IsAny<DtoWorkItemData>()))
                .Returns((DtoWorkItemData wi) =>
                {
                    createdWis.Add(wi);
                    return wi;
                });
            stgprocess.Setup(x => x.MoveWorkItemInProcess(
                    It.IsAny<DtoActivityProcessDefinition>(), It.IsAny<STGDocument>(), It.IsAny<DtoWorkItemData>(), It.IsAny<ISTGConfiguration>()))
                .Returns((DtoActivityProcessDefinition procDef, STGDocument stgDoc, DtoWorkItemData wi, ISTGConfiguration config) =>
                {
                    createdDocs[wi] = stgDoc;
                    return wi;
                });
            stgprocess.Setup(x => x.CommitWorkItem(It.IsAny<DtoWorkItemData>(), It.IsAny<STGDocument>()))
                .Callback((DtoWorkItemData wi, STGDocument stgDoc) =>
                {
                    createdDocs[wi] = stgDoc;
                });

            // Run test
            activity.Process(stgprocess.Object, Mock.Of<ISTGConfiguration>());

            Assert.That(createdWis.Count, Is.EqualTo(2));
            Assert.That(createdDocs.Where(x => x.Value.Media.First().Extension == ".tiff").ToList().Count, Is.EqualTo(1));
            Assert.That(createdDocs.Where(x => x.Value.Media.First().Extension == ".jpg").ToList().Count, Is.EqualTo(1));
        }
    }
}
