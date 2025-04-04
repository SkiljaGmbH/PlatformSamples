using STG.Common.DTO;
using STG.Common.DTO.Metadata;
using STG.RT.API.Activity;
using STG.RT.API.Document;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SampleActivities
{
    public class SplitActivity : STGUnattendedAbstract<SplitActivity.Config>
    {
        public override void Process(DtoWorkItemData workItemInProgress, STGDocument documentToProcess)
        {
            var children = documentToProcess.ChildDocuments.Select(x => x.ID).ToList();

            if (children.Any())
            {
                var workItems = documentToProcess.CreateWorkItemsForChildren(children);
                ChildWorkItems = workItems;
                var plural = ChildWorkItems.Count > 1 ? "children" : "child";
                workItemInProgress.Message = $"split off {children.Count} {plural}";
            }
        }

        public class Config : ActivityConfigBase<Config>
        {
            public Config()
            {
                PlatformDataDesc = "This activity splits off the child documents from the root document into separate work items. Each child document that was split off will become the root document of a new work item. All work items will be moved along in the process.";
            }

            [Display(Name = "Split Activity", Description = "Showcases how a single work item can be split up into multiple work items.")]
            [InputType(InputType.textarea), ReadOnly(true)]
            public string PlatformDataDesc { get; set; }
        }
    }
}
