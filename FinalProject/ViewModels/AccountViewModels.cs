using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static FinalProject.Shared.Constants;

namespace FinalProject.ViewModels
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "Endereço de e-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password, ErrorMessage = "Senha inválida")]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Display(Name = "Lembrar?")]
        public bool RememberMe { get; set; }
    }

    public class BaseProfileInfoViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Endereço de e-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O seu primeiro nome é obrigatório")]
        [DataType(DataType.Text)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.",
            MinimumLength = MinPasswordLength)]
        [DataType(DataType.Password, ErrorMessage = "Senha inválida")]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required(ErrorMessage = "A seleção de uma formação é obrigatória")]
        [Display(Name = "Selecione sua formação atual")]
        public int Grade { get; set; }

        [Display(Name = "Formação completa?")]
        public bool IsGradeCompleted { get; set; }

        public IList<GradeListItemViewModel> Grades { get; set; }
    }

    public class RegisterViewModel : BaseProfileInfoViewModel
    {
        [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
        [DataType(DataType.Password, ErrorMessage = "Confirmação de senha inválida")]
        [Display(Name = "Confirme a senha")]
        [Compare("Password", ErrorMessage = "As senhas são diferentes.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterInstitutionManagerViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Endereço de e-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O primeiro nome é obrigatório")]
        [DataType(DataType.Text)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "A seleção de uma instituição é obrigatória")]
        [Display(Name = "Selecione uma instituição*")]
        public int InstitutionId { get; set; }

        public int InstitutionIndex { get; set; }

        public IList<InstitutionViewModel> Institutions { get; set; }

        public bool IsRequest { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }
    }

    public class EditViewModel : BaseProfileInfoViewModel
    {
        [DataType(DataType.Password, ErrorMessage = "Confirmação de senha inválida")]
        [Display(Name = "Confirme a nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas são diferentes.")]
        public string ConfirmNewPassword { get; set; }

        [Required(ErrorMessage = "A seleção de uma instituição é obrigatória")]
        [Display(Name = "Selecione uma instituição")]
        public int Institution { get; set; }

        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.",
            MinimumLength = MinPasswordLength)]
        [DataType(DataType.Password, ErrorMessage = "Senha inválida")]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }
        
        public IList<string> Tags { get; set; }

        public IList<InstitutionViewModel> Institutions { get; set; }
    }

    public class DetailsViewModel
    {
        public string Email { get; set; }

        public string Id { get; set; }

        public string Institution { get; set; }

        public string Name { get; set; }

        public string ProfilePicture { get; set; }

        public IList<string> Tags { get; set; }

        public bool IsCurrentUser { get; set; }

        public string UserId { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Code { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
        [DataType(DataType.Password, ErrorMessage = "Confirmação de senha inválida")]
        [Display(Name = "Confirme a senha")]
        [Compare("Password", ErrorMessage = "As senhas são diferentes.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.",
            MinimumLength = MinPasswordLength)]
        [DataType(DataType.Password, ErrorMessage = "Senha inválida")]
        [Display(Name = "Senha")]
        public string Password { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}