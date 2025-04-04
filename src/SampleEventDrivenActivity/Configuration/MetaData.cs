using System.Collections.Generic;

namespace SampleEventDrivenActivity.Configuration
{
    public class MetaData
    {
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public List<Indexfield> IndexFields { get; set; } = new List<Indexfield>();
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<CustomValue> CustomValues { get; set; } = new List<CustomValue>();
    }
}