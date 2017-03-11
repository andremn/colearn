using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FinalProject.ViewModels
{
    public class PreferencesViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public float MinSimilarity { get; set; }

        [Required]
        public float MinRating { get; set; }

        public InstitutionSelectItemViewModel[] SelectInstitutions { get; set; }

        public string[] Institutions { get; set; }

        public GradeListItemViewModel[] SelectGrades { get; set; }

        public int? Grade { get; set; }

        [Display(Name = "Ano")]
        [Required(ErrorMessage = "O ano é obrigatorio")]
        public ushort? GradeYear { get; set; }

        [Display(Name = "Formação completa?")]
        public bool IsGradeCompleted { get; set; }
    }

    public class StudentSearchListItemViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profilePic")]
        public string ProfilePic { get; set; }

        [JsonProperty("institution")]
        public string Institution { get; set; }

        [JsonProperty("avgRating")]
        public string AvgRating { get; set; }

        [JsonProperty("ratingsCoutn")]
        public long RatingsCount { get; set; }

        [JsonProperty("questionTags")]
        public IList<TagViewModel> QuestionTags { get; set; }

        [JsonProperty("instructorTags")]
        public IList<TagViewModel> InstructorTags { get; set; }

        [JsonProperty("similarity")]
        public string Similarity { get; set; }

        [JsonProperty("institutions")]
        public string[] Institutions { get; set; }
        
        [JsonProperty("grade")]
        public string Grade { get; set; }
    }

    public class StudentSearchViewModel
    {
        [Display(Name = "Nome")]
        public string StudentName { get; set; }

        [Display(Name = "Avaliação média mínima")]
        public float StudentMinAvgRating { get; set; }

        [Display(Name = "Instituições")]
        public IList<InstitutionSelectItemViewModel> SelectInstitutions { get; set; }

        public IList<int> Institutions  { get; set; }

        [Display(Name = "Formação")]
        public IList<GradeListItemViewModel> SelectGrades { get; set; }
        
        public IList<int> Grades { get; set; }
    }

    public class StudentSearchListViewModel
    {
        public ICollection<StudentSearchListItemViewModel> Students { get; set; }
    }
}