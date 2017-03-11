using System.Collections.Generic;

namespace FinalProject.Model
{
    public class StudentDataTransfer : IDataTransfer
    {
        public string Email { get; set; }

        public string FirstName { get; set; }
        
        public int Id { get; set; }

        public InstitutionDataTransfer Institution { get; set; }

        public IList<TagAcceptedDataTransfer> InstructorTags { get; set; }

        public bool IsModerator { get; set; }

        public string LastName { get; set; }

        public IList<InstitutionDataTransfer> ModeratingInstitutions { get; set; }

        public string ProfilePictureId { get; set; }

        public IList<QuestionDataTransfer> Questions { get; set; }

        public float AvgRating { get; set; }

        public IList<TagAcceptedDataTransfer> QuestionTags { get; set; }

        public GradeDataTransfer Grade { get; set; }

        public ushort GradeYear { get; set; }
    }
}
