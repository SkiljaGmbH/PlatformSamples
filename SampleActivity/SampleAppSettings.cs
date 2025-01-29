using SampleActivity.Settings;
using STG.Common.DTO;
using STG.Common.Utilities.Settings;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    public class SampleAppSettings : STGUnattendedAbstract<AppSettingsSettings>
    {
        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
            string result = string.Empty;
            var settings = AppSettingsFactory.Default.GetSettings();
            result += "system.auth.client_id" + ": " + settings.ReadString("system.auth.client_id") + "; ";

            var settingNames = ActivityConfiguration.AppSettingsToOutput.Split(',');

            foreach (var settingName in settingNames)
            {
                result += settingName + ": " + settings.ReadString(settingName) + "; ";
            }

            documentToProcess.AddCustomValue(ActivityConfiguration.OutputCustomValue, result, true);
        }
    }
}
