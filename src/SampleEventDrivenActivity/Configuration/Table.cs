using System.Collections.Generic;

namespace SampleEventDrivenActivity.Configuration
{
    public class Table
    {
        public string Name { get; set; }
        public List<string> Header { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }
}