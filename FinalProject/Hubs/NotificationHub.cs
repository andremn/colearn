using System;
using FinalProject.ViewModels;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace FinalProject.Hubs
{
    public class NotificationHub : BaseHub
    {
        public static void NotifyClientsNewNotification(NotificationViewModel notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }
            
            var result = new
            {
                count = 1,
                notifications = new[]
                {
                    notification
                }
            };

            var notificationJson = JsonConvert.SerializeObject(result);
            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            if (!string.IsNullOrWhiteSpace(notification.UserId))
            {
                var connectionId = GetConnectionId(notification.UserId);

                if (connectionId != null)
                {
                    notificationHub.Clients.Client(connectionId).newNotification(notificationJson);
                }
            }
            else
            {
                notificationHub.Clients.All.newNotification(notificationJson);
            }
        }
    }
}