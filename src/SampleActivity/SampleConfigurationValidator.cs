using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    public class SampleConfigurationValidator : STGUnattendedAbstract<ConfigurationValidationSettings>
    {
        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
            // do nothing, this is just for configuration validation
        }
    }
}
