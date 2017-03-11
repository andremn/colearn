using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class AgendaHub : BaseHub
    {
        public static void NotifyUserCalendarChanged(int userId)
        {
            var questionHub = GlobalHost.ConnectionManager.GetHubContext<AgendaHub>();

            questionHub.Clients.All.userCalendarChanged(userId);
        }
    }
}