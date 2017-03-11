using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinalProject.LocalResource;

namespace FinalProject.ViewModels
{
    public class ManageGradesViewModel
    {
        [Display(ResourceType = typeof(Resource), Name = "GradeNameFieldPlaceholder")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CreateGradeViewModelNameFieldRequiredErrorMessage")]
        public string Name { get; set; }

        public IList<GradeListItemViewModel> Grades { get; set; }
    }

    public class GradeListItemViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CreateGradeViewModelNameFieldRequiredErrorMessage")]
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public int Order { get; set; }
    }
}