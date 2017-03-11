using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FinalProject.Context;
using FinalProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class BaseHub : Hub
    {
        public static ConcurrentDictionary<string, string> ConnectedUsers = 
            new ConcurrentDictionary<string, string>();

        private ApplicationUserManager UserManager { get; } =
            new ApplicationUserManager(new UserStore<User>(new UserDbContext()));


        public override async Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var userId = Context.User.Identity.GetUserId();
                var connectionId = Context.ConnectionId;

                if (!ConnectedUsers.ContainsKey(userId))
                {
                    ConnectedUsers.TryAdd(userId, connectionId);
                }
            }

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var userId = Context.User.Identity.GetUserId();

                if (ConnectedUsers.ContainsKey(userId))
                {
                    string connectionId;

                    ConnectedUsers.TryRemove(userId, out connectionId);
                }

                await UpdateLastSeenOnlineAsync(userId);
            }

            await base.OnDisconnected(stopCalled);
        }

        protected static string GetConnectionId(string userId)
        {
            string connectionId;

            return ConnectedUsers.TryGetValue(userId, out connectionId) 
                ? connectionId 
                : null;
        }

        private async Task UpdateLastSeenOnlineAsync(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);

            user.LastSeenOnline = DateTime.UtcNow;
            await UserManager.UpdateAsync(user);
        }
    }
}