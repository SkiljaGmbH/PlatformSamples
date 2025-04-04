using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class SampleJpgToTiffConverter : STGUnattendedAbstract<CommonSettings>
    {
        /// <summary>
        /// Set this member to local document API factory in unit tests in order to avoid accessing the database.
        /// </summary>
        public IDocumentApiFactory DocumentApiFactory { get; set; } = ClientFactory.Default.CreateDocumentApiFactory();

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            if (document.Media.Count > 0)
            {
                STGMedia jpegMedia = document.Media.FirstOrDefault(m => m.MediaType.MediaTypeName.Equals("jpg", StringComparison.OrdinalIgnoreCase));

                Image img = Image.FromStream(jpegMedia.MediaStream);
                MemoryStream convertedStream = new MemoryStream();
                img.Save(convertedStream, System.Drawing.Imaging.ImageFormat.Tiff);


                DtoMediaType tiffMediaType = DocumentApiFactory.CreateMediaTypeService().LoadAvailableMediaTypes()
                        .FirstOrDefault(mt => mt.MediaTypeName.Equals("tiff", StringComparison.OrdinalIgnoreCase));
                STGMedia tiffMEdia = STGMedia.Initialize("tiffMedia", ".tif", tiffMediaType, convertedStream);

                document.AppendMedia(tiffMEdia);
            }
        }
    }
}
