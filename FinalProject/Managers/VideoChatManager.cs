using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FinalProject.Helpers;
using WebGrease.Css.Extensions;

namespace FinalProject.Managers
{
    public class VideoChatManager : IVideoChatManager
    {
        private static readonly IDictionary<long, VideoChatManager> Managers;

        private static readonly object VideoChatsLock = new object();
        
        private readonly VideoChat _videoChat;

        static VideoChatManager()
        {
            Managers = new ConcurrentDictionary<long, VideoChatManager>();
        }

        private VideoChatManager(long chatId)
        {
            _videoChat = new VideoChat(chatId);
        }

        /// <summary>
        /// Gets an instance of <see cref="IVideoChatManager"/> for the specified chat.
        /// </summary>
        /// <param name="chatId">The id of the chat to get a manager for; 
        /// if it is 0, a new manager will be created and an id will be auto generated.</param>
        /// <returns><see cref="IVideoChatManager"/></returns>
        public static IVideoChatManager GetInstanceFor(long chatId)
        {
            VideoChatManager manager;

            if (chatId != 0)
            {
                return !Managers.TryGetValue(chatId, out manager) ? null : manager;
            }

            var id = DateTimeHelper.GetNowUnixTimeStampMilliseconds();

            manager = new VideoChatManager(id);
            Managers[id] = manager;

            return manager;
        }

        public static bool IsVideoChatInitialized(long chatId)
        {
            VideoChatManager manager;

            return Managers.TryGetValue(chatId, out manager) && manager.IsInitialized;
        }

        public long ChatId => _videoChat.Id;

        public bool IsInitialized { get; private set; }

        public void InitVideoChat(IList<string> participants)
        {
            lock (VideoChatsLock)
            {
                _videoChat.Participants = participants;
                _videoChat.PendingParticipants = new List<string>(participants);
                IsInitialized = true;
            }
        }

        public IReadOnlyList<string> Participants
        {
            get
            {
                lock (VideoChatsLock)
                {
                    return _videoChat.Participants.ToSafeReadOnlyCollection();
                }
            }
        }

        public string Presenter
        {
            get
            {
                lock (VideoChatsLock)
                {
                    return _videoChat.Presenter;
                }
            }
            set
            {
                lock (VideoChatsLock)
                {
                    if (_videoChat.Presenter != value)
                    {
                        _videoChat.Presenter = value;
                    }
                }
            }
        }

        public int? QuestionId
        {
            get
            {
                lock (VideoChatsLock)
                {
                    return _videoChat.QuestionId;
                }
            }
            set
            {
                lock (VideoChatsLock)
                {
                    if (_videoChat.QuestionId != value)
                    {
                        _videoChat.QuestionId = value;
                    }
                }
            }
        }

        public int? AnswerId
        {
            get
            {
                lock (VideoChatsLock)
                {
                    return _videoChat.AnswerId;
                }
            }
            set
            {
                lock (VideoChatsLock)
                {
                    if (_videoChat.AnswerId != value)
                    {
                        _videoChat.AnswerId = value;
                    }
                }
            }
        }

        public bool AddParticipant(string userId)
        {
            lock (VideoChatsLock)
            {
                var participants = _videoChat.PendingParticipants;

                if (!participants.Contains(userId))
                {
                    throw new InvalidOperationException(
                        $"Participant {userId} is not allowed to enter in video chat {ChatId}.");
                }

                if (participants.Count == 0)
                {
                    throw new InvalidOperationException("Cannot add a participant after the video chat has started.");
                }

                participants.Remove(userId);

                return participants.Count == 0;
            }
        }

        private class VideoChat
        {
            public VideoChat(long id)
            {
                Id = id;
            }

            public long Id { get; }

            public string Presenter { get; set; }

            public IList<string> Participants { get; set; }

            public IList<string> PendingParticipants { get; set; }

            public int? QuestionId { get; set; }

            public int? AnswerId { get; set; }
        }
    }
}