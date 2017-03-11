using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Service.Notification
{
    public class NotificationListener : IListener
    {
        public static NotificationListener Instance { get; } = new NotificationListener();

        private static readonly object DelegatesSync = new object();

        private readonly IDictionary<string, IList<NotificationListenerDelegate>> _delegates;

        private NotificationListener()
        {
            _delegates = new Dictionary<string, IList<NotificationListenerDelegate>>();
        }

        public void AddListenerForCategory(string categoryName, 
            NotificationListenerDelegate notificationListenerDelegate)
        {
            if (notificationListenerDelegate == null)
            {
                throw new ArgumentNullException(nameof(notificationListenerDelegate));
            }

            lock (DelegatesSync)
            {
                IList<NotificationListenerDelegate> delegateList;

                if (_delegates.TryGetValue(categoryName, out delegateList))
                {
                    delegateList.Add(notificationListenerDelegate);
                    return;
                }

                delegateList = new List<NotificationListenerDelegate>
                {
                    notificationListenerDelegate
                };

                _delegates[categoryName] = delegateList;
            }
        }

        internal void NotifyCategoryListenersAsync(string categoryName,
            NotificationEvent notificationData)
        {
            Task.Run(() =>
            {
                IList<NotificationListenerDelegate> delegateList;

                lock (DelegatesSync)
                {
                    if (!_delegates.TryGetValue(categoryName, out delegateList))
                    {
                        return;
                    }
                }

                foreach (var listenerDelegate in delegateList)
                {
                    listenerDelegate?.Invoke(this, notificationData);
                }
            });
        }
    }
}