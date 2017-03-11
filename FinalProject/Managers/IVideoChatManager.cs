using System.Collections.Generic;

namespace FinalProject.Managers
{
    public interface IVideoChatManager
    {
        long ChatId { get; }

        bool IsInitialized { get; }

        void InitVideoChat(IList<string> participants);

        IReadOnlyList<string> Participants { get; }
        
        string Presenter { get; set; }

        int? QuestionId { get; set; }

        int? AnswerId { get; set; }

        bool AddParticipant(string userId);
    }
}