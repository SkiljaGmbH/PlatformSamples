using SampleEventDrivenActivity.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDAClient.Data
{
    public class MetaDataExtended : MetaData
    {
        public string ClassificationURL { get; set; }
        public string VCPURL { get; set; }
        public bool DoDocClassification { get; set; }
        public string  DocClassificationClassCV { get; set; }
        public bool DoPageClassification { get; set; }
        public string PageClassificationClassCV { get; set; }
    }
}
