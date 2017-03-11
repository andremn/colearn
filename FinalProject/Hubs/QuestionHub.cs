using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class QuestionHub : BaseHub
    {
        public static void NotifyClientNewAnswer(string userId)
        {
            var questionHub = GlobalHost.ConnectionManager.GetHubContext<QuestionHub>();
            var connectionId = GetConnectionId(userId);

            if (connectionId != null)
            {
                questionHub.Clients.Client(connectionId).newAnswer();
            }
        }

        public static void NotifyNewAnswer(string data)
        {
            var questionHub = GlobalHost.ConnectionManager.GetHubContext<QuestionHub>();

            questionHub.Clients.All.onNewAnswer(data);
        }
    }
}