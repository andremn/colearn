using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Model
{
    public enum TagRequestStatus
    {
        Pending = 0,

        Approved = 1,

        Denied = 2
    }

    public class TagRequest : IModel
    {
        public virtual Student Author { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Institution Institution { get; set; }

        public virtual Question Question { get; set; }

        public TagRequestStatus Status { get; set; }

        [MaxLength(50)]
        [Index(IsUnique = false)]
        public string Text { get; set; }
    }
}