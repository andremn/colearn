namespace FinalProject.Service.Notification
{
    public delegate void NotificationListenerDelegate(IListener sender, NotificationEvent notificationEvent);

    public interface IListener
    {
        void AddListenerForCategory(string categoryName, NotificationListenerDelegate notificationListenerDelegate);
    }
}