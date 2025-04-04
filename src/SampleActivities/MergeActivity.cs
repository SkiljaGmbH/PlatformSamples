using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using STG.RT.API.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SampleActivities
{
    public class MergeActivity : STGProcessTimerAbstract<MergeActivity.Config>
    {
        public override void Process(ISTGProcess processLayer, ISTGConfiguration configurationLayer)
        {
            var processDefinition = configurationLayer.LoadActivityInstanceProcessSettings(ActivityInfo);

            var workItems = processLayer.GetAvailableWorkItemsForInstance(ActivityInfo.ActivityInstanceID, 50);

            var parentWorkItems = workItems.Where(x => x.ParentWorkItemID.HasValue == false).ToList();
            foreach (var parentWorkItem in parentWorkItems)
            {
                var childWorkItems = processLayer.GetChildWorkItems(parentWorkItem);

                //Make sure all child work items are in this activity instance and ready
                if (!childWorkItems.Any(x => x.ActivityInstanceID != ActivityInfo.ActivityInstanceID || x.Status != DtoWorkItemStatus.Ready))
                {
                    var lockedParent = processLayer.LockWorkItem(parentWorkItem);
                    var lockedChildWorkItems = processLayer.LockWorkItems(childWorkItems);

                    var mergedWorkItem = processLayer.MergeChildWorkItems(lockedParent, lockedChildWorkItems);

                    var document = STGDocument.Load(mergedWorkItem);
                    processLayer.MoveWorkItemInProcess(processDefinition, document, mergedWorkItem, configurationLayer);
                }
            }
        }

        public class Config : ActivityConfigBase<Config>
        {
            public Config()
            {
                PlatformDataDesc = "This activity demonstrates how multiple work items can be merged into a single work item.";
            }

            [Display(Name = "Merge Activity", Description = "Showcases how work items can be merged")]
            [InputType(InputType.textarea), ReadOnly(true)]
            public string PlatformDataDesc { get; set; }
        }
    }
}
