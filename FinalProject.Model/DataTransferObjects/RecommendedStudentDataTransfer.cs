using System.Collections.Generic;

namespace FinalProject.Model
{
    public class RecommendedStudentDataTransfer
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public int Id { get; set; }

        public InstitutionDataTransfer Institution { get; set; }

        public IList<TagAcceptedDataTransfer> InstructorTags { get; set; }
        
        public string LastName { get; set; }

        public string ProfilePictureId { get; set; }

        public IList<TagAcceptedDataTransfer> QuestionTags { get; set; }

        public float AvgRating { get; set; }

        public float Similarity { get; set; }

        public GradeDataTransfer Grade { get; set; }

        public ushort GradeYear { get; set; }

        public long RatingsCount { get; set; }
    }
}