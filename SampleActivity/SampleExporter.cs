using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    public class SampleExporter : STGUnattendedAbstract<ExportSettings>
    {
        string _exportPath;

        public override void Initialize(Stream configuration)
        {
            base.Initialize(configuration);

            // need to fill the dictionary here instead of the configuration class constructor because the JSON serialization calls the default constructor
            if (ActivityConfiguration.ExportMediaMappings == null ||
                ActivityConfiguration.ExportMediaMappings.Keys.Count == 0)
            {
                if (ActivityConfiguration.ExportMediaMappings == null)
                {
                    ActivityConfiguration.ExportMediaMappings = new SerializableDictionary<string, string>();
                }

                ActivityConfiguration.ExportMediaMappings.Add("PDF", ".pdf");
                ActivityConfiguration.ExportMediaMappings.Add("Tiff", ".tif");
                ActivityConfiguration.ExportMediaMappings.Add("Jpg", ".jpg");
                ActivityConfiguration.ExportMediaMappings.Add("Text", ".ocr");
            }
        }

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            _exportPath = ActivityConfiguration.FlatExport ? ActivityConfiguration.ExportPath : Path.Combine(ActivityConfiguration.ExportPath, workItemInProgress.WorkItemID.ToString());
            if (!Directory.Exists(_exportPath))
            {
                Directory.CreateDirectory(_exportPath);
            }

            DoExport(document);

            document.AddCustomValue("SampleExporter", DateTime.Now.ToString("yyyyMMddHHmmss"), true);
        }

        void DoExport(STGDocument document)
        {
            var sw = new Stopwatch();
            sw.Restart();
            foreach (var item in ActivityConfiguration.ExportMediaMappings)
            {
                var typeMedias = document.Media.Where(m => m.MediaType.MediaTypeName.Equals(item.Key, StringComparison.OrdinalIgnoreCase));

                foreach (STGMedia media in typeMedias)
                {
                    string fileName = string.Format("{0}_{1}{2}", document.ID, media.ID, item.Value);
                    var fileStream = File.Create(Path.Combine(_exportPath, fileName));

                    media.MediaStream.CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            sw.Stop();
            document.AddCustomValue("ExportTime", sw.Elapsed.TotalMilliseconds.ToString(), false);

            foreach (var child in document.ChildDocuments)
            {
                DoExport(child);
            }
        }
    }
}
