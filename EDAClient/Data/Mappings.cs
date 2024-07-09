using System.Collections.Generic;
using SampleEventDrivenActivity.Configuration;

namespace EDAClient.Data
{
    internal class Results
    {
        public Results()
        {
            Fields = new List<Indexfield>();
        }

        public string Name { get; set; }
        public IList<Indexfield> Fields { get; }
    }
}