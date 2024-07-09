using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.Common.Utilities.Settings;

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
