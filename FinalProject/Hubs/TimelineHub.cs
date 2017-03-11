using FinalProject.ViewModels;
using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class TimelineHub : BaseHub
    {
        public static void NotifyNewQuestionCreated(TimelineItemViewModel item)
        {
            var timelineHubContext = GlobalHost.ConnectionManager.GetHubContext<TimelineHub>();

            timelineHubContext.Clients.All.onNewItemAdded(item);
        }
    }
}