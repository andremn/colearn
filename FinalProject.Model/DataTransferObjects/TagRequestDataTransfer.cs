namespace FinalProject.Model
{
    public class TagRequestDataTransfer : TagDataTransfer
    {
        public StudentDataTransfer Author { get; set; }
        
        public InstitutionDataTransfer Institution { get; set; }

        public QuestionDataTransfer Question { get; set; }

        public TagRequestStatus Status { get; set; }
    }
}
