using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public enum QuestionStatus
    {
        Created = 0,

        PendingApproval = 1,

        Deleted = 2
    }

    public class Question : IModel
    {
        public virtual IList<Answer> Answers { get; set; }

        public virtual Student Author { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Institution Institution { get; set; }

        public QuestionStatus Status { get; set; }

        public virtual IList<Tag> Tags { get; set; }

        public string Title { get; set; }
    }
}