using Newtonsoft.Json;
using SampleEventDrivenActivity.Configuration;
using STG.Common.DTO;
using STG.Common.DTO.Document;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SampleEventDrivenActivity
{
    public class EventDrivenNotifier : STGEventDrivenResponderAbstract<EventDrivenNotifierSettings>
    {
        /// <summary>
        /// Generates the activity definition that can be used on EDA clients to understand the result stream better.
        /// The EDA client still needs to know what this stream could be (in this case, it's a json).
        /// </summary>
        public override Stream GenerateStreamDefinition()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);

            var result = new NotifierSettings();
            result.ExportCustomExtensions = ActivityConfiguration.ExportExtensions;
            result.ExportFieldsAndTables = ActivityConfiguration.ExportFieldsAndTables;
            result.ExportMedia = ActivityConfiguration.ExportMedia;

            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                using (var jsonTextWriter = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jsonTextWriter, result);
                }
            }

            return stream;
        }

        /// <summary>
        /// Converts a document into a response stream for this EDA response activity.
        /// Called when a work item reaches this activity.
        /// </summary>
        public override Stream Process(DtoWorkItemData workItem, STGDocument document)
        {
            var memoryStream = new MemoryStream();
            // The created zip file is going to be the result stream - the corresponding EDA client needs to understand this zip data contract
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                AddToArchive(archive, document, "");
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Adds document data to a zip file.
        /// </summary>
        public void AddToArchive(ZipArchive archive, STGDocument document, string zipDirectoryName)
        {
            AddMediaToZip(archive, document, zipDirectoryName);

            var response = CreateResponseMetaData(document);

            var newEntry = archive.CreateEntry(zipDirectoryName + "metadata.json");
            AddMetadataToZipEntry(newEntry, response);

            foreach (var childDoc in document.ChildDocuments)
            {
                // simple recursion to traverse the document tree
                AddToArchive(archive, childDoc, zipDirectoryName + childDoc.ID + "/");
            }
        }

        private void AddMediaToZip(ZipArchive archive, STGDocument document, string zipDirectoryName)
        {
            if (ActivityConfiguration.ExportMedia == false)
                return;

            foreach (var stgMedia in document.Media)
            {
                // Names are not unique, therefore append the unique ID
                var name = zipDirectoryName + $"{stgMedia.Name}_{stgMedia.ID}";
                if (!string.IsNullOrWhiteSpace(stgMedia.Extension))
                {
                    name = $"{name}.{stgMedia.Extension.TrimStart('.')}";
                }
                var entry = archive.CreateEntry(name);
                using (var entryStream = entry.Open())
                {
                    // in more complex scenarios, the media stream might have to be rewound.
                    if (stgMedia.MediaStream.CanSeek && stgMedia.MediaStream.Position != 0)
                        stgMedia.MediaStream.Position = 0;
                    stgMedia.MediaStream.CopyTo(entryStream);
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="ResponseMetaData"/> object for the stream result
        /// </summary>
        public ResponseMetaData CreateResponseMetaData(STGDocument document)
        {
            var response = new ResponseMetaData();
            response.DocumentName = document.Name;
            response.DocumentType = document.DocumentType;
            foreach (var cv in document.CustomValues)
            {
                response.CustomValues.Add(new CustomValue() { Key = cv.Key, Value = cv.Value });
            }

            FillFieldsAndTables(response, document);

            AddExtensions(response, document);
            return response;
        }


        private static void AddMetadataToZipEntry(ZipArchiveEntry newEntry, ResponseMetaData response)
        {
            using (var content = newEntry.Open())
            using (StreamWriter writer = new StreamWriter(content))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, response);
                jsonWriter.Flush();
            }
        }

        public void AddExtensions(ResponseMetaData response, STGDocument document)
        {
            if (ActivityConfiguration.ExportExtensions == false)
                return;
            // Usually, extensions are accessed via their type, but we don't have those here => get all type names
            var extTypes = document.GetExtensionTypes();
            foreach (var ext in extTypes)
            {
                // The stream is usually a json and can be used as such
                var e = document.GetExtensionData(ext);
                using (var streamReader = new StreamReader(e))
                {
                    var txt = streamReader.ReadToEnd();

                    response.Extensions[ext] = txt;
                }
            }
        }

        private void FillFieldsAndTables(ResponseMetaData response, STGDocument document)
        {
            if (ActivityConfiguration.ExportFieldsAndTables == false)
                return;
            foreach (var field in document.IndexFields)
            {
                var indexfield = new Indexfield();
                indexfield.Name = field.FieldName;
                indexfield.Value = field.FieldValue.GetText();
                response.IndexFields.Add(indexfield);
            }

            foreach (var table in document.Tables)
            {
                var t = new Table();
                t.Name = table.TableName;
                foreach (var stgColumnDefinition in table.ColumnDefinition)
                {
                    t.Header.Add(stgColumnDefinition.Name);
                }

                foreach (var tableRow in table.Rows)
                {
                    var row = new List<string>();
                    t.Rows.Add(row);
                    foreach (var tableRowCell in tableRow.Cells)
                    {
                        row.Add(tableRowCell.CellValue.GetText());
                    }
                }

                response.Tables.Add(t);
            }
        }
    }

    public class NotifierSettings
    {
        public bool ExportCustomExtensions { get; set; }
        public bool ExportFieldsAndTables { get; set; }
        public bool ExportMedia { get; set; }
        public IReadOnlyCollection<DtoDocumentType> DocumentTypes { get; set; } = new List<DtoDocumentType>();
    }
}
