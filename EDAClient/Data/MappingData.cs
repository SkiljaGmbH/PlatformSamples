using System.Collections.Generic;
using System.IO;
using EDAClient.Common;
using Newtonsoft.Json;

namespace EDAClient.Data
{
    internal class MappingData
    {
        public MappingData()
        {
            DocumentMappings = new List<DocumentMapping>();
        }

        public IList<DocumentMapping> DocumentMappings { get; }

        internal static MappingData ReadForActivity(int activityID)
        {
            var ret = new MappingData();
            var settingsPath = SharedData.IOService.GetDataFilePath(activityID);
            if (File.Exists(settingsPath))
                using (var rdr = new StreamReader(File.Open(settingsPath, FileMode.Open)))
                {
                    using (var jtr = new JsonTextReader(rdr))
                    {
                        ret = new JsonSerializer().Deserialize<MappingData>(jtr);
                    }
                }

            return ret;
        }
    }

    internal class DocumentMapping
    {
        public DocumentMapping()
        {
            FieldMappings = new List<FieldMapping>();
        }

        public string DocumentTypeName { get; set; }

        public IList<FieldMapping> FieldMappings { get; }
    }

    internal class FieldMapping
    {
        public string FieldSource { get; set; }
        public string DestinationName { get; set; }
    }
}