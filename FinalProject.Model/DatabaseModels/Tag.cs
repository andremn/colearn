using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Model
{
    public class Tag : IModel
    {
        public virtual IList<Tag> Children { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Institution Institution { get; set; }

        public virtual IList<Student> Instructors { get; set; }

        public virtual IList<Tag> Parents { get; set; }

        public virtual IList<Question> Questions { get; set; }

        [MaxLength(50)]
        [Index(IsUnique = false)]
        public string Text { get; set; }
    }
}