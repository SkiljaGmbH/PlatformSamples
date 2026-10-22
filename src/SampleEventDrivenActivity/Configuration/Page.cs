using System;
using System.Collections.Generic;

namespace SampleEventDrivenActivity.Configuration
{
    public class Page
    {
        public Guid ID { get; set; }
        public List<Guid> AssignedMedia { get; set; } = new List<Guid>();
        public Guid DefaultMedia { get; set; } = Guid.Empty;
        public int Index { get; set; }

        public Dictionary<string, string> PageExtensions { get; set; } = new Dictionary<string, string>();
    }
}