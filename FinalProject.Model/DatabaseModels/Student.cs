using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static FinalProject.Shared.Constants;

namespace FinalProject.Model
{
    public class Student : IModel
    {
        [Index(IsUnique = true)]
        [MaxLength(MaxEmailAddressLength)]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public virtual Grade Grade { get; set; }

        public short GradeYear { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Institution Institution { get; set; }

        public virtual IList<Tag> InstructorTags { get; set; }

        public bool IsModerator { get; set; }

        public string LastName { get; set; }

        public virtual IList<Institution> ModeratingInstitutions { get; set; }

        public string ProfilePictureId { get; set; }

        public virtual IList<Question> Questions { get; set; }
    }
}