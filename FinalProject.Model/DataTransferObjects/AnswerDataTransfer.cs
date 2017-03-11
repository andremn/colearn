using System;
using System.Collections.Generic;

namespace FinalProject.Model
{
    public class AnswerDataTransfer : IDataTransfer
    {
        public StudentDataTransfer Author { get; set; }

        public DateTime CreatedDate { get; set; }
        
        public int Id { get; set; }

        public QuestionDataTransfer Question { get; set; }

        public IList<AnswerRatingDataTransfer> Ratings { get; set; }
    }

    public class TextAnswerDataTransfer : AnswerDataTransfer
    {
        public string Text { get; set; }
    }

    public class VideoAnswerDataTransfer : AnswerDataTransfer
    {
        public string VideoFilePath { get; set; }
    }
}
