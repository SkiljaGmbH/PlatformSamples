using SampleActivity.Configuration;
using SampleActivity.Properties;
using STG.RT.API.Activity;
using System.IO;

namespace SampleActivity
{
    public class SampleLocalizationDefaults : STGExternalAbstract<Localizer>
    {
        /// <summary>
        /// Demonstrates how to set the default value in the Initialize method
        /// </summary>
        /// <param name="configuration"></param>
        public override void Initialize(Stream configuration)
        {
            base.Initialize(configuration);
            ActivityConfiguration.DescFromInit = Resources.Localizer_DescFromInit_Prompt;
        }
    }
}
