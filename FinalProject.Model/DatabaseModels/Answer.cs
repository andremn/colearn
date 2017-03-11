using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public class Answer : IModel
    {
        public virtual Student Author { get; set; }

        public DateTime CreatedDate { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Question Question { get; set; }

        public virtual IList<AnswerRating> Ratings { get; set; }
    }
}