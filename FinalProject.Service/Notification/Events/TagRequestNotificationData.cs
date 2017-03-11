using FinalProject.Model;

namespace FinalProject.Service.Notification
{
    public class TagRequestNotificationData : INotificationData
    {
        public TagRequestNotificationData(TagRequestDataTransfer tagRequest, int pendingRequestsCount)
        {
            TagRequest = tagRequest;
            PendingRequestsCount = pendingRequestsCount;
        }

        public TagRequestDataTransfer TagRequest { get; set; }

        public int PendingRequestsCount { get; set; }
    }
}
