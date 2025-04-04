using Newtonsoft.Json;
using SampleEventDrivenActivity.Configuration;
using STG.Common.DTO;
using STG.Common.Interfaces.Document;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.RT.API.Document.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SampleEventDrivenActivity
{
    public class EventDrivenInitializer : STGEventDrivenInitializerAbstract<EventDrivenInitializerSettings>
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

            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                using (var jsonTextWriter = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jsonTextWriter, ActivityConfiguration);
                }
            }

            return stream;
        }

        /// <summary>
        /// Converts an stream sent to an event driven activity into an SGDocument.
        /// The work item is created by the hosting process for this particular stream. The stream is deleted after the document is returned
        /// </summary>
        public override STGDocument Process(DtoWorkItemData workItem, Stream eventStream)
        {
            // Use the factory to create a new document (new'ing would also work, but makes unit testing difficult)
            var doc = DocumentApiFactory.CreateDocumentFactory().CreateSTGDoc(workItem);
            using (var ms = new MemoryStream())
            {
                eventStream.CopyTo(ms);
                ms.Position = 0;
                FillDocumentFromZipStructure(doc, ms);
            }

            return doc;
        }

        /// <summary>
        /// The <see cref="DocumentApiFactory"/> makes it possible to control document creation in unit tests.
        /// </summary>
        public DocumentApiFactory DocumentApiFactory { get; set; } = new DocumentApiFactory(new DocumentFactoryOptions());

        /// <summary>
        /// The default media type service would require services; like this, we can work offline if the <see cref="DocumentApiFactory"/> is configured like that.
        /// </summary>
        public ISTGMediaTypeService STGMediaTypeService => DocumentApiFactory.CreateMediaTypeService();

        /// <summary>
        /// Reads the stream as a zip file and creates an STGDocument from it. The zip file structure cannot be arbitrary, of course.
        /// </summary>
        public void FillDocumentFromZipStructure(STGDocument rootDocument, MemoryStream memoryStream)
        {
            using (var archive = new ZipArchive(memoryStream))
            {
                // wrap the zip entries in a little helper class to ease further implementation
                var entries = archive.Entries.Select(e => (IEntryHelper)new EntryHelper(e)).Where(e => string.IsNullOrEmpty(e.FileName) == false).ToList();
                var rootDirectory = string.Empty;
                HandleCurrentLvl(rootDocument, entries, rootDirectory);
                HandleSubLevels(rootDocument, entries, rootDirectory);
            }
        }

        /// <summary>
        /// Recursively traverse the document tree and create child documents for each subfolder
        /// </summary>
        private void HandleSubLevels(STGDocument document, List<IEntryHelper> entries, string currentDir)
        {
            var subDirectories = GroupSubDirectories(entries, currentDir);
            foreach (var subDirEntries in subDirectories)
            {
                var child = document.AppendChild();
                var subDir = Path.Combine(currentDir, GetBaseDir(subDirEntries[0], currentDir));
                HandleCurrentLvl(child, subDirEntries, subDir);
                HandleSubLevels(child, subDirEntries, subDir);
            }
        }

        private string GetBaseDir(IEntryHelper subDirEntry, string currentDir)
        {
            var dir = subDirEntry.DirectoryName.Substring(currentDir.Length);
            var dirParts = dir.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            if (dirParts.Length > 0)
                return dirParts[0];
            return "";
        }

        /// <summary>
        /// Create an <see cref="STGDocument"/> from the data on the <paramref name="currentDir"/> in the zip file.
        /// </summary>
        /// <param name="document">The document to fill</param>
        /// <param name="entries">All zip archive entries on this level</param>
        /// <param name="currentDir">The current working directory in the zip file</param>
        private void HandleCurrentLvl(STGDocument document, List<IEntryHelper> entries, string currentDir)
        {
            var currentFiles = entries.Where(n => n.DirectoryName == currentDir).ToList();
            var mediaEntries2 = currentFiles.Where(n => IsMetaDataFile(n.FileName) == false);
            foreach (var mediaEntry in mediaEntries2)
            {
                var media = CreateMedia(mediaEntry);
                if (media != null)
                {
                    document.AppendMedia(media);
                    if (ActivityConfiguration.CreatePages)
                    {
                        var page = document.AppendPage(new STGPage());
                        page.AppendMedia(media, null, true);
                    }
                }
            }

            // By default, the document is named as the subdirectory (or root in case of top level)
            document.Name = GetCurrentDirName(currentDir);
            var metaData = GetMetaData(currentFiles);
            ApplyMetaData(document, metaData);
        }

        /// <summary>
        /// Reads the metadata file if present. The file name is reserved: 'metadata.json'
        /// </summary>
        private MetaData GetMetaData(List<IEntryHelper> currentFiles)
        {
            var entry = currentFiles.FirstOrDefault(x => IsMetaDataFile(x.FileName));
            if (entry == null)
                return null;

            var meta = DeserializeJson<MetaData>(entry.Open());
            return meta;
        }

        private string GetCurrentDirName(string currentDir)
        {
            if (string.IsNullOrEmpty(currentDir))
            {
                return string.IsNullOrEmpty(ActivityConfiguration.RootDocumentName) ? "Root" : ActivityConfiguration.RootDocumentName;
            }

            var dirParts = currentDir.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            return dirParts.Any() ? dirParts.Last() : currentDir;
        }

        public List<List<IEntryHelper>> GroupSubDirectories(List<IEntryHelper> entries, string currentLevel)
        {
            var hits = entries.Where(x => x.DirectoryName.StartsWith(currentLevel) && x.DirectoryName != currentLevel);
            var levels = hits.GroupBy(x => x.DirectoryName);
            levels = hits.GroupBy(x =>
            {
                var fullSubDir = x.DirectoryName.Substring(currentLevel.Length);
                var parts = fullSubDir.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                return parts[0];
            });
            var orderedEnumerable = levels.OrderBy(x => x.Key).ToList();
            return orderedEnumerable.Select(x => x.ToList()).ToList();
        }

        /// <summary>
        /// Fill the documents fields/tables/custom values with the content from <paramref name="metaData"/>.
        /// </summary>
        public void ApplyMetaData(STGDocument doc, MetaData metaData)
        {
            if (metaData == null)
                return;

            if (string.IsNullOrEmpty(metaData.DocumentName) == false)
                doc.Name = metaData.DocumentName;
            if (string.IsNullOrEmpty(metaData.DocumentType) == false)
            {
                doc.Initialize(metaData.DocumentType);
                foreach (var metaIndexField in metaData.IndexFields)
                {
                    var field = doc.IndexFields.FirstOrDefault(x => string.Equals(x.FieldName, metaIndexField.Name, StringComparison.InvariantCultureIgnoreCase));
                    field?.FieldValue.SetText(metaIndexField.Value);
                }

                foreach (var metaTable in metaData.Tables)
                {
                    var table = doc.Tables.FirstOrDefault(x => string.Equals(x.TableName, metaTable.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (table == null)
                        continue;

                    foreach (var metaTableRow in metaTable.Rows)
                    {
                        var row = table.InsertNewRow();
                        int index = 0;
                        foreach (var headerColumn in metaTable.Header)
                        {
                            var col = row.Cells.FirstOrDefault(x => string.Equals(x.ColumnName, headerColumn, StringComparison.InvariantCultureIgnoreCase));
                            if (col == null)
                                continue;
                            col.CellValue.SetText(metaTableRow[index]);
                            index++;
                        }
                    }
                }
            }

            foreach (var customValue in metaData.CustomValues)
            {
                doc.AddCustomValue(customValue.Key, customValue.Value, false);
            }
        }

        private static T DeserializeJson<T>(Stream stream)
        {
            var serializer = JsonSerializer.Create();
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        private static bool IsMetaDataFile(string filename)
        {
            return string.Equals(filename, "Metadata.json", StringComparison.InvariantCultureIgnoreCase);
        }

        public STGMedia CreateMedia(IEntryHelper entry)
        {
            var mediaType = STGMediaTypeService.GetMediaType(entry.ExtensionWithoutDot);
            if (mediaType == null)
            {
                if (ActivityConfiguration.HandleUnknownExtensions == UnknownMediaExtensionHandling.ImportAsUnknown)
                    mediaType = STGMediaTypeService.GetMediaType("UnknownBinaryFile");
                else return null;
            }
            var builder = new STGMediaBuilder();
            var ms = CreateStreamCopy(entry.Open());
            var media = builder.CreateMedia(entry.FileName, entry.Extension, mediaType)
                .WithStream(ms).ReleaseStreamOwnership().Finish();
            return media;
        }

        private Stream CreateStreamCopy(Stream open)
        {
            var ms = new MemoryStream();
            open.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
