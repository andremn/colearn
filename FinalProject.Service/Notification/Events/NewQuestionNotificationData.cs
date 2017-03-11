using FinalProject.Model;

namespace FinalProject.Service.Notification
{
    public class NewQuestionNotificationData : INotificationData
    {
        public NewQuestionNotificationData(QuestionDataTransfer question)
        {
            Question = question;
        }

        public QuestionDataTransfer Question { get; set; }
    }
}
