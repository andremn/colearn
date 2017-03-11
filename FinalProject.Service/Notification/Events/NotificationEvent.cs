namespace FinalProject.Service.Notification
{
    public class NotificationEvent
    {
        public NotificationEvent(INotificationData data)
        {
            Data = data;
        }

        public INotificationData Data { get; }
    }
}
