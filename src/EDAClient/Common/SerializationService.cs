using System.IO;
using Newtonsoft.Json;

namespace EDAClient.Common
{
    internal class SerializationService
    {
        public T ReadFromStream<T>(Stream streamDefinition)
        {
            var ret = default(T);
            using (var stmr = new StreamReader(streamDefinition))
            using (var jtr = new JsonTextReader(stmr))
            {
                var serializer = new JsonSerializer();
                ret = serializer.Deserialize<T>(jtr);
            }

            return ret;
        }

        public T ReadFromFromFile<T>(string filePath)
        {
            if (File.Exists(filePath))
                using (var strm = File.Open(filePath, FileMode.Open))
                {
                    return ReadFromStream<T>(strm);
                }

            return default(T);
        }

        public void WriteToFileFile<T>(T dataToSave, string filePath)
        {
            new FileInfo(filePath).Directory.Create();

            using (var sw = new StreamWriter(File.Open(filePath, FileMode.OpenOrCreate)))
            {
                using (var jtw = new JsonTextWriter(sw))
                {
                    new JsonSerializer().Serialize(jtw, dataToSave);
                }
            }
        }
    }
}