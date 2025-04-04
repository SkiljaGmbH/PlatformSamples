using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.Common.Interfaces.Document;
using STG.RT.API;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.RT.API.Interfaces;

namespace SampleActivity
{
    public class SampleImportTimer : STGProcessTimerAbstract<ImportSettings>
    {
        /// <summary>
        /// Set this member to local document API factory in unit tests in order to avoid accessing the database.
        /// </summary>
        public IDocumentApiFactory DocumentApiFactory { get; set; } = ClientFactory.Default.CreateDocumentApiFactory();

        public override void Process(ISTGProcess processLayer, ISTGConfiguration configurationLayer)
        {
            DirectoryInfo di = new DirectoryInfo(ActivityConfiguration.ImportPath);

            FileInfo[] tifs = di.GetFiles(ActivityConfiguration.TiffFilter);
            FileInfo[] jpgs = di.GetFiles(ActivityConfiguration.JpegFilter);

            var mediaTypes = DocumentApiFactory.CreateMediaTypeService().LoadAvailableMediaTypes();
            DtoMediaType tiffMediaType = mediaTypes.First(m => m.MediaTypeName.Equals("tiff", StringComparison.OrdinalIgnoreCase));
            DtoMediaType jpegMediaType = mediaTypes.First(m => m.MediaTypeName.Equals("jpg", StringComparison.OrdinalIgnoreCase));

            DtoActivityProcessDefinition processSettings = configurationLayer.LoadActivityInstanceProcessSettings(ActivityInfo);

            foreach (FileInfo tiffFile in tifs)
            {
                var input = new DtoWorkItemData() { ProcessID = ActivityInfo.Process.ProcessID, ActivityInstanceID = ActivityInfo.ActivityInstanceID };
                DtoWorkItemData newWorkItem = processLayer.CreateWorkItem(input);
                STGDocument rootDoc = DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(newWorkItem);
                STGMedia tiffMedia = STGMedia.Initialize("TiffMedia", tiffMediaType, tiffFile.FullName, true);
                rootDoc.AppendMedia(tiffMedia);

                rootDoc.AddCustomValue(ActivityConfiguration.Common.RoutingCustomValueName, "tif", false);


                if (!string.IsNullOrWhiteSpace(this.ActivityConfiguration.DocumentType))
                {
                    var docTypeDefinition = rootDoc.LoadDocTypeByName(this.ActivityConfiguration.DocumentType);
                    rootDoc.Initialize(docTypeDefinition);
                }

                processLayer.MoveWorkItemInProcess(processSettings, rootDoc, newWorkItem, configurationLayer);
            }

            foreach (FileInfo jpegFile in jpgs)
            {
                var input = new DtoWorkItemData() { ProcessID = ActivityInfo.Process.ProcessID, ActivityInstanceID = ActivityInfo.ActivityInstanceID };
                DtoWorkItemData newWorkItem = processLayer.CreateWorkItem(input);
                STGDocument rootDoc = DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(newWorkItem);

                STGMedia jpgMedia = STGMedia.Initialize("jpegMedia", jpegMediaType, jpegFile.FullName, true);
                rootDoc.AppendMedia(jpgMedia);
                var page = rootDoc.AppendPage(new STGPage());
                page.AppendMedia(jpgMedia, new STGImageBasedPageLocation(jpgMedia.ID, 0), false);

                rootDoc.AddCustomValue(ActivityConfiguration.Common.RoutingCustomValueName, "jpg", false);
                rootDoc.AddCustomValue(ActivityConfiguration.Common.FilteringCustomValueName, "1", false);

                processLayer.MoveWorkItemInProcess(processSettings, rootDoc, newWorkItem, configurationLayer);
            }

        }
    }
}
