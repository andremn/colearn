using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class VideoChatHub : BaseHub
    {
        public static void CallParticipants(
            long chatId, 
            IEnumerable<string> participants, 
            IReadOnlyList<string> otherParticipantsNames)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<VideoChatHub>();

            foreach (var participant in participants)
            {
                var connectionId = ConnectedUsers[participant];
                var clientConnection = hubContext.Clients.Client(connectionId);

                clientConnection.videoChatCalling(
                    otherParticipantsNames[0],
                    chatId);
            }
        }

        public static void InitCall(long chatId, IEnumerable<string> participants)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<VideoChatHub>();

            foreach (var participant in participants)
            {
                var connectionId = ConnectedUsers[participant];
                var clientConnection = hubContext.Clients.Client(connectionId);

                clientConnection.videoChatStarted(chatId);
            }
        }

        public static void EndCall(long chatId, IEnumerable<string> participants)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<VideoChatHub>();

            foreach (var participant in participants)
            {
                var connectionId = ConnectedUsers[participant];
                var clientConnection = hubContext.Clients.Client(connectionId);

                clientConnection.videoChatEnded(chatId);
            }
        }

        public static void CallError(long chatId, IEnumerable<string> participants, string error)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<VideoChatHub>();

            foreach (var participant in participants)
            {
                var connectionId = ConnectedUsers[participant];
                var clientConnection = hubContext.Clients.Client(connectionId);

                clientConnection.videoChatError(chatId, error);
            }
        }

        public static void RefuseCall(long chatId, string user, IEnumerable<string> participants)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<VideoChatHub>();

            foreach (var participant in participants)
            {
                var connectionId = ConnectedUsers[participant];
                var clientConnection = hubContext.Clients.Client(connectionId);

                clientConnection.videoChatRefused(user, chatId);
            }
        }
    }
}