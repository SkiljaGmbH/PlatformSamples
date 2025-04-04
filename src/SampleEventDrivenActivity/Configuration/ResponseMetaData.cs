using System.Collections.Generic;

namespace SampleEventDrivenActivity.Configuration
{
    public class ResponseMetaData : MetaData
    {
        public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
    }
}