namespace FinalProject.Hubs
{
    public class WebRtcHub : BaseHub
    {
        public void PropagateDrawing(long chatId, string drawing)
        {
            Clients.Others.updateDrawing(chatId, drawing);
        }

        public void Send(long chatId, string message)
        {
            Clients.Others.newMessage(chatId, message);
        }
    }
}