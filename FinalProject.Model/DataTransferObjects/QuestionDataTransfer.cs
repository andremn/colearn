using System;
using System.Collections.Generic;

namespace FinalProject.Model
{
    public class QuestionDataTransfer : IDataTransfer
    {
        public StudentDataTransfer Author { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }
        
        public int Id { get; set; }

        public InstitutionDataTransfer Institution { get; set; }

        public QuestionStatus Status { get; set; }

        public string Title { get; set; }

        public IList<TagAcceptedDataTransfer> Tags { get; set; }

        public IList<AnswerDataTransfer> Answers { get; set; }
    }
}