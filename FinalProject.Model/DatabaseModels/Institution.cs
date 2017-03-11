using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Model
{
    public class Institution : IModel
    {
        [Index(IsUnique = true)]
        public int Code { get; set; }

        public string FullName { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual IList<Student> Moderators { get; set; }

        public virtual IList<Question> Questions { get; set; }

        public string ShortName { get; set; }

        public virtual IList<Tag> Tags { get; set; }

        public override string ToString()
        {
            return $"{Code} - {FullName} ({ShortName})";
        }
    }
}