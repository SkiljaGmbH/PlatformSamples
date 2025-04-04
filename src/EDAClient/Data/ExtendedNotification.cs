using STG.Common.DTO.EventDriven;

namespace EDAClient.Data
{
    public class ExtendedNotification : DtoEventDrivenNotification
    {
        public ExtendedNotification(DtoEventDrivenNotification source)
        {
            ID = source.ID;
            Message = source.Message;
            Status = source.Status;
            WorkItemID = source.WorkItemID;
            ActivityInstanceID = source.ActivityInstanceID;
            CreationTime = source.CreationTime;
            RelatedStream = source.RelatedStream;
            TimeStamp = source.TimeStamp;
        }
        public bool OurNotification { get; set; }
    }
}