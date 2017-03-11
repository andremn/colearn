using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace FinalProject.Hubs
{
    public class ConnectionStatusHub : BaseHub
    {
        public bool IsUserOnline(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            string connectionId;

            var isOnline =  ConnectedUsers.TryGetValue(userId, out connectionId);

            return isOnline;
        }

        public override async Task OnConnected()
        {
            await base.OnConnected();

            var userId = Context.User.Identity.GetUserId();

            Clients.All.statusChanged(userId, true);
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            await base.OnDisconnected(stopCalled);

            var userId = Context.User.Identity.GetUserId();

            Clients.All.statusChanged(userId, false);
        }
    }
}