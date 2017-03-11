using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int Order { get; set; }
    }
}
