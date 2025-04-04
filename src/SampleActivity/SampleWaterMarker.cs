using System;
using System.Drawing;
using System.IO;
using System.Linq;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    class SampleWaterMarker : STGUnattendedAbstract<WatermarkSettings>
    {
        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            var documentToProcess = document as STGDocument;
            if (documentToProcess.Media.Count > 0)
            {
                STGMedia tiffMedia = documentToProcess.Media.FirstOrDefault(m => m.MediaType.MediaTypeName.Equals("tiff", StringComparison.OrdinalIgnoreCase));

                MemoryStream mediaStream = new MemoryStream();
                tiffMedia.MediaStream.CopyTo(mediaStream);

                //watermark it here
                using (Image img = Image.FromStream(mediaStream))
                {

                    //Select the active page
                    img.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, 0);

                    using (Bitmap bmp = new Bitmap(img.Width, img.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(img, 0, 0, img.Width, img.Height);
                            Brush myBrush = new SolidBrush(Color.FromArgb(127, Color.Red));

                            // Calculate the size of the text
                            Font myFont = new Font(new FontFamily("Arial"), 48);
                            SizeF sz = g.MeasureString(ActivityConfiguration.WatermarkText, myFont);
                            g.DrawString(ActivityConfiguration.WatermarkText, myFont, myBrush, new PointF(0, 0));
                            g.Save();

                            bmp.Save(mediaStream, img.RawFormat);

                            mediaStream.Seek(0, SeekOrigin.Begin);

                            tiffMedia.MediaStream = mediaStream;
                        }
                    }
                }
                // Get a graphics context

            }
        }
    }
}
