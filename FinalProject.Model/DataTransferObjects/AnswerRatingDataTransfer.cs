namespace FinalProject.Model
{
    public class AnswerRatingDataTransfer : IDataTransfer
    {
        public AnswerDataTransfer Answer { get; set; }

        public StudentDataTransfer Author { get; set; }

        public int Id { get; set; }

        public float Value { get; set; }
    }
}
