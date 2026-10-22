using System.Collections.Generic;

namespace SampleEventDrivenActivity.Configuration
{
    public class ResponseMetaData : MetaData
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
    }
}