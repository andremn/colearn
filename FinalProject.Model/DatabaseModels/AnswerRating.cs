using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public class AnswerRating : IModel
    {
        public Answer Answer { get; set; }

        public Student Author { get; set; }

        [Key]
        public int Id { get; set; }

        public float Value { get; set; }
    }
}