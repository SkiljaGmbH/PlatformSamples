using EDAClient.Data;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using STG.RT.API.Interfaces;
using System.IO;

namespace EDAClient.Common
{
    internal static class SharedData
    {
        static SharedData()
        {
            if (File.Exists("OEM.json"))
            {
                using (StreamReader file = File.OpenText(@"OEM.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    OEM = (OEM)serializer.Deserialize(file, typeof(OEM));
                }
            }
            else
            {
                OEM = new OEM();
            }
        }

        public static ISnackbarMessageQueue SnackBarMessageQ { get; set; }

        public static ISTGEventDriven EDA { get; set; }

        public static IOService IOService { get; } = new IOService();

        public static SerializationService Serialization { get; } = new SerializationService();

        public static MessagingService MessagingService { get; set; }

        public static OEM OEM { get; private set; }
        public static IClientFactory ClientFactory { get; internal set; }
    }
}