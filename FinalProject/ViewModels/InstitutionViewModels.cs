using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    public class SelectInstitutionViewModel
    {
        [Required(ErrorMessage = "Uma instituição deve ser selecionada")]
        [Display(Name = "Selecione uma instituição*")]
        public int Institution { get; set; }

        public IList<InstitutionViewModel> Institutions { get; set; }
    }

    public class InstitutionViewModel
    {
        public int Code { get; set; }

        public string FullName { get; set; }

        public int Id { get; set; }

        public bool IsRequest { get; set; }

        public string ModeratorEmail { get; set; }

        public string ShortName { get; set; }

        public string RequesterEmail { get; set; }
    }

    public class InstitutionSelectItemViewModel
    {
        public int Code { get; set; }

        public string FullName { get; set; }

        public int Id { get; set; }

        public bool IsSelected { get; set; }

        public string ShortName { get; set; }
    }

    public class RegisterInstitutionViewModel
    {
        [Required(ErrorMessage = "O código da instituição é obrigatório")]
        [Display(Name = "Código")]
        public int Code { get; set; }

        [Required(ErrorMessage = "O nome completo da instituição é obrigatório")]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "A sigla da instituição é obrigatória")]
        [Display(Name = "Sigla")]
        public string ShortName { get; set; }
    }

    public class DenyInstitutionRequestViewModel
    {
        public int InstitutionRequestId { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [Display(Name = "E-mail do autor do pedido")]
        public string ManagerEmail { get; set; }

        [Required(ErrorMessage = "O motivo é obrigatório")]
        [Display(Name = "Motivo para não aceitar o pedido")]
        public string Reason { get; set; }
    }
}