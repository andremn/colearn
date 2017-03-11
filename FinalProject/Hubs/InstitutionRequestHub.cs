using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class InstitutionRequestHub : BaseHub
    {
        public static void NotifyNewInstitutionRequestCountUpdated(int newCount)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<InstitutionRequestHub>();

            context.Clients.All.institutionRequestsUpdated(newCount);
        }
    }
}