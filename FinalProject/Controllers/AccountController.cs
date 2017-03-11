using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.DataAccess.Filters;
using FinalProject.Extensions;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Models;
using FinalProject.Service;
using FinalProject.Service.Admin;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using static FinalProject.Shared.Constants;

namespace FinalProject.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private IStudentService _studentService;

        private ApplicationRoleManager _roleManager;

        private ApplicationSignInManager _signInManager;

        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public IStudentService StudentService
        {
            get { return _studentService ?? GetService<IStudentService>(); }

            set { _studentService = value; }
        }

        public ApplicationRoleManager RoleManager
        {
            get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }

            set { _roleManager = value; }
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }

            private set { _signInManager = value; }
        }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddInstructorTags(string tags)
        {
            var student = await GetCurrentStudentAsync();
            var parsedTags = tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var addedTags = new List<string>();
            var notAddedTags = new List<string>();
            var tagService = GetService<ITagService>();

            foreach (var tag in parsedTags)
            {
                var acceptedTag = await tagService.GetTagByNameForInstitutionAsync(tag, student.Institution.Id);

                if (acceptedTag != null)
                {
                    addedTags.Add(tag);
                    student.InstructorTags.Add(acceptedTag);
                    await StudentService.UpdateStudentAsync(student);
                }
                else
                {
                    var tagRequest = new TagRequestDataTransfer
                    {
                        Text = tag,
                        Author = student,
                        Institution = student.Institution
                    };

                    notAddedTags.Add(tag);
                    await tagService.CreateTagRequestAsync(tagRequest);
                }
            }

            string pendingTagsMessage = null;

            if (notAddedTags.Count == 1)
            {
                pendingTagsMessage = string.Format(Resource.InstructorTagNotAdded, notAddedTags[0]);
            }
            else if (notAddedTags.Count > 1)
            {
                var pendingTags = new StringBuilder();
                int i;

                for (i = 0; i < notAddedTags.Count - 1; i++)
                {
                    pendingTags.Append($"'{notAddedTags[i]}'");
                }

                pendingTags.Append($" e '{notAddedTags[i]}'");
                pendingTagsMessage = string.Format(Resource.InstructorTagsNotAdded, pendingTags);
            }

            return
                Json(
                    new
                    {
                        error = string.Empty,
                        success = true,
                        notAddedTagsMessage = pendingTagsMessage,
                        html = this.RenderViewToString("_InstructorTagPartial", addedTags)
                    });
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // GET: /Account/ConfirmEmailConfirmation
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmailConfirmation(
            string userId,
            string code,
            bool createPassword = false)
        {
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (!result.Succeeded)
            {
                return View("Error");
            }

            if (createPassword)
            {
                return RedirectToAction(
                    "ResetPassword",
                    new { code = await UserManager.GeneratePasswordResetTokenAsync(userId) });
            }

            var user = await UserManager.FindByIdAsync(userId);

            Session["UserEmail"] = user.Email;
            return View();
        }

        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            var student = await GetCurrentStudentAsync();
            bool isCurrentStudent;

            if (!id.HasValue)
            {
                isCurrentStudent = true;
            }
            else
            {
                isCurrentStudent = student.Id == id;

                if (!isCurrentStudent)
                {
                    student = await StudentService.GetStudentByIdAsync(id.Value);
                }
            }

            if (student == null)
            {
                return View("Error");
            }

            var user = await UserManager.FindByEmailAsync(student.Email);

            var model = new DetailsViewModel
            {
                Institution = $"{student.Institution.FullName} ({student.Institution.ShortName})",
                Id = student.Id.ToString(),
                Email = student.Email,
                Name = $"{student.FirstName} {student.LastName}",
                ProfilePicture = student.GetProfilePicture(),
                Tags = student.InstructorTags.Select(t => t.Text).OrderBy(t => t).ToList(),
                IsCurrentUser = isCurrentStudent,
                UserId = user.Id
            };

            return View(model);
        }

        // GET: /Account/Edit
        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var student = await GetCurrentStudentAsync();

            var institutions = await GetService<IInstitutionService>().GetAllInstitutionsAsync();
            var institutionViewModels = institutions.Select(i => new InstitutionViewModel
            {
                Id = i.Id,
                Code = i.Code,
                FullName = i.FullName,
                IsRequest = false,
                ShortName = i.ShortName
            }).ToList();

            var grades = await GetService<IGradeService>().GetAllGradesAsync();
            var gradeViewModels = grades.Select(g => new GradeListItemViewModel
            {
                Id = g.Id,
                Name = g.Name,
                IsSelected = student.Grade.Id == g.Id
            }).ToList();

            var model = new EditViewModel
            {
                Email = student.Email,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Institution = student.Institution.Id,
                Tags = student.InstructorTags.Select(t => t.Text).OrderBy(t => t).ToList(),
                Institutions = institutionViewModels,
                Grades = gradeViewModels
            };

            return View(model);
        }

        // POST: /Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Edit();
            }

            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                SignInManager.AuthenticationManager.SignOut();
                return RedirectToAction("Login");
            }

            var institutionService = GetService<IInstitutionService>();
            var student = await StudentService.GetStudentByEmailAsync(user.Email);
            var institution = await institutionService.GetInstitutionByIdAsync(model.Institution);

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.Institution = institution;

            await StudentService.UpdateStudentAsync(student);

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return await Edit();
            }

            user.Email = model.Email;

            var updateResult = await UserManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
            }

            var passwordChangeResult =
                await UserManager.ChangePasswordAsync(user.Id, model.Password, model.NewPassword);

            if (!passwordChangeResult.Succeeded)
            {
                AddErrors(passwordChangeResult);
            }

            return await Edit();
        }

        // GET: /Account/RegisterInstitutionManager
        [HttpPost]
        [Authorize(Roles = UserRoles.SystemAdminRole)]
        public async Task<ActionResult> FindStudentDetails(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { Found = false });
            }

            var student = await StudentService.GetStudentByEmailAsync(email);

            if (student == null)
            {
                return Json(new { Found = false });
            }

            return Json(new { Found = true, student.FirstName, student.LastName, student.Email });
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword(string email)
        {
            return View(new ForgotPasswordViewModel { Email = email ?? string.Empty });
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null || !await UserManager.IsEmailConfirmedAsync(user.Id))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation", model);
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // ReSharper disable once PossibleNullReferenceException
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, Request.Url.Scheme);
            var student = await StudentService.GetStudentByEmailAsync(user.Email);

            if (student != null)
            {
                await UserManager.SendEmailAsync(
                    user.Id,
                    "Alterar senha",
                    $"<p>Olá {student.FirstName},</p><p>Você pode redefinir sua senha clicando <a href=\"" + callbackUrl
                    + "\">aqui.</a></p>");

                return View("ForgotPasswordConfirmation", model);
            }

            // Send email to admin
            var message = new IdentityMessage
            {
                Body = $"Clique <a href='{callbackUrl}'>aqui</a> para recuperar sua senha",
                Destination = user.Email,
                Subject = "Alterar senha"
            };

            await GetService<IEmailService>().SendAsync(message);
            return View("ForgotPasswordConfirmation", model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation(ForgotPasswordViewModel model)
        {
            return model == null ? View("ForgotPassword") : View(model);
        }

        // GET: /Account/Index
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole(UserRoles.SystemAdminRole))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole(UserRoles.StudentRole))
                {
                    return RedirectToAction("Index", "Timeline");
                }

                if (User.IsInRole(UserRoles.InstitutionModeratorRole))
                {
                    return RedirectToAction("Index", "Tag");
                }
            }

            ViewBag.ReturnUrl = returnUrl;

            var userEmail = Session["UserEmail"] as string;

            return userEmail == null ? View() : View(new LoginViewModel { Email = userEmail });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await
                    SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);

                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("Email", Resource.NotVerifiedEmailErrorMessage);
                        return View(model);
                    }

                    var student = await GetStudentByUserIdAsync(user.Id);

                    if (student?.Institution != null)
                    {
                        var institutionCookie = new HttpCookie(
                            "StudentInstitution",
                            student.Institution.Id.ToString());

                        Response.SetCookie(institutionCookie);
                    }

                    return await RedirectToLocalAsync(user.Id, returnUrl);
                default:
                    ModelState.AddModelError(string.Empty, Resource.WrongUserOrPasswordErrorMessage);
                    return View(model);
            }
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Response.Cookies.Remove("StudentInstitution");
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Register()
        {
            var grades = await GetService<IGradeService>().GetAllGradesAsync();
            var gradeViewModels = grades.Select(g => new GradeListItemViewModel
            {
                Id = g.Id,
                Name = g.Name
            }).ToArray();

            return View(new RegisterViewModel
            {
                Grades = gradeViewModels
            });
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User { UserName = model.Email, Email = model.Email };

            var result = await UserManager.CreateAsync(user, model.Password);
            StudentDataTransfer student = null;

            if (result.Succeeded)
            {
                var createdUser = await UserManager.FindByEmailAsync(user.Email);

                try
                {
                    if (!await RoleManager.RoleExistsAsync(UserRoles.StudentRole))
                    {
                        await RoleManager.CreateAsync(new IdentityRole(UserRoles.StudentRole));
                    }

                    await UserManager.AddToRoleAsync(createdUser.Id, UserRoles.StudentRole);

                    // Creates the student
                    student = await StudentService.CreateStudentAsync(new StudentDataTransfer
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Grade = new GradeDataTransfer { Id = model.Grade }
                    });
                }
                catch
                {
                    await UserManager.DeleteAsync(user);

                    if (student != null)
                    {
                        await StudentService.DeleteStudentAsync(student);
                    }

                    throw;
                }

                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                // ReSharper disable once PossibleNullReferenceException
                var callbackUrl = Url.Action(
                    "ConfirmEmailConfirmation",
                    "Account",
                    new { userId = user.Id, code, createPassword = false },
                    Request.Url.Scheme);

                var emailService = GetService<IEmailService>();

                await emailService.SendForStudentAsync(
                        new StudentDataTransfer { Email = model.Email, FirstName = model.FirstName },
                        string.Format(Resource.AccountResponseEmailSubject, Resource.ProductName),
                        string.Format(Resource.ConfirmAccountEmailMessage, callbackUrl));

                ViewBag.Email = model.Email;
                return View("ConfirmEmail");
            }

            var errors = new List<string>();
            var hasEmailError = false;

            foreach (var error in result.Errors)
            {
                if (error.Contains(user.Email))
                {
                    hasEmailError = true;
                    continue;
                }

                errors.Add(error);
            }

            if (hasEmailError)
            {
                errors.Add($"O email {user.Email} já está sendo usado.");
            }

            result = new IdentityResult(errors);

            AddErrors(result);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveInstructorTag(string tag)
        {
            var student = await GetCurrentStudentAsync();
            var tagObj = student.InstructorTags
                .SingleOrDefault(t => t.Text.Equals(tag, StringComparison.OrdinalIgnoreCase));

            // ReSharper disable once InvertIf
            if (tagObj != null)
            {
                student.InstructorTags.Remove(tagObj);
                await StudentService.UpdateStudentAsync(student);
                return Json(new { error = string.Empty, success = true, tag = tagObj.Text });
            }

            return Json(new { error = string.Empty, success = true, tag });
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string code)
        {
            var user = await UserManager.FindByIdAsync(userId);

            return code == null || user == null
                ? View("Error")
                : View(new ResetPasswordViewModel { Code = code, Email = user.Email });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var creatingPassword = user.PasswordHash == null;
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)
            {
                if (!creatingPassword)
                {
                    Session["UserEmail"] = user.Email;
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                await SignInManager.SignInAsync(user, false, false);
                return RedirectToAction("Login", "Account");
            }

            AddErrors(result);
            return View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/SelectInstitution
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.StudentRole)]
        public async Task<ActionResult> SelectInstitution()
        {
            var institutions = await GetService<IInstitutionService>().GetAllInstitutionsAsync();
            var institutionModels = institutions
                .Select(i => new InstitutionViewModel
                {
                    Id = i.Id,
                    Code = i.Code,
                    FullName = i.FullName,
                    IsRequest = false,
                    ShortName = i.ShortName
                }).ToList();

            return View(new SelectInstitutionViewModel
            {
                Institutions = institutionModels
            });
        }

        // POST: /Account/SelectInstitution
        [HttpPost]
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.StudentRole)]
        public async Task<ActionResult> SelectInstitution(SelectInstitutionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await GetCurrentStudentAsync();
            var institution = await GetService<IInstitutionService>().GetInstitutionByIdAsync(model.Institution);

            student.Institution = institution;
            await StudentService.UpdateStudentAsync(student);

            return RedirectToAction("Index", "Timeline");
        }

        // GET: /Account/RegisterInstitutionManager
        [HttpGet]
        [Authorize(Roles = UserRoles.SystemAdminRole)]
        public async Task<ActionResult> SetInstitutionManager(int? requestId, bool? isInstitutionRequest)
        {
            var isRequest = isInstitutionRequest.GetValueOrDefault(false);
            var institutions = await GetRequestInstitutionViewModelsAsync(isRequest);

            if (!requestId.HasValue)
            {
                return View(new RegisterInstitutionManagerViewModel
                {
                    InstitutionIndex = -1,
                    InstitutionId = 0,
                    Institutions = institutions
                });
            }

            var institution = institutions.SingleOrDefault(i => i.IsRequest == isRequest && i.Id == requestId.Value);
            var index = institutions.IndexOf(institution);

            return View(new RegisterInstitutionManagerViewModel
            {
                InstitutionIndex = index,
                InstitutionId = requestId.Value,
                Institutions = institutions,
                IsRequest = isRequest
            });
        }

        // POST: Account/SetInstitutionManager
        [HttpPost]
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.InstitutionModeratorRole)]
        public async Task<ActionResult> SetInstitutionManager(RegisterInstitutionManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Institutions = await GetRequestInstitutionViewModelsAsync(model.IsRequest);
                return View(model);
            }

            var institutionService = GetService<IInstitutionService>();
            InstitutionDataTransfer institution;

            if (model.IsRequest)
            {
                var institutionRequest = await institutionService.GetInstitutionRequestByIdAsync(model.InstitutionId);

                if (institutionRequest != null && institutionRequest.Status == InstitutionRequestStatus.Pending)
                {
                    institutionRequest.Status = InstitutionRequestStatus.Approved;
                    await institutionService.UpdateInstitutionRequestAsync(institutionRequest);

                    institution = new InstitutionDataTransfer
                    {
                        FullName = institutionRequest.InstitutionFullName,
                        ShortName = institutionRequest.InstitutionShortName,
                        Code = institutionRequest.InstitutionCode
                    };

                    institution = await institutionService.CreateInstitutionAsync(institution);
                }
                else
                {
                    throw new Exception();
                }

                model.Institutions = await GetRequestInstitutionViewModelsAsync(true);
            }
            else
            {
                institution = await institutionService.GetInstitutionByIdAsync(model.InstitutionId);
                model.Institutions = await GetRequestInstitutionViewModelsAsync(false);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                return await SetInstitutionManager(user, model, institution, false);
            }

            // User does not exist, so we'll create a new one and send him/her an email
            user = new User { UserName = model.Email, Email = model.Email, EmailConfirmed = true };

            var result = await UserManager.CreateAsync(user);

            if (result.Succeeded)
            {
                return await SetInstitutionManager(user, model, institution, true);
            }

            AddErrors(result);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetProfilePicture(HttpPostedFileBase picture)
        {
            var student = await GetCurrentStudentAsync();

            if (string.IsNullOrEmpty(student.ProfilePictureId) && 
                (picture == null || picture.ContentLength == 0))
            {
                student.ProfilePictureId = null;
                await StudentService.UpdateStudentAsync(student);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            if (string.IsNullOrEmpty(student.ProfilePictureId))
            {
                var picId = Guid.NewGuid();

                student.ProfilePictureId = picId.ToString();
                await StudentService.UpdateStudentAsync(student);
            }

            var destinationPath = Server.MapPath($"{ProfilePicturesFolderVirtualPath}/{student.ProfilePictureId}.png");

            picture.SaveAs(destinationPath);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private async Task<IList<InstitutionViewModel>> GetRequestInstitutionViewModelsAsync(bool isRequest)
        {
            if (isRequest)
            {
                var filter = new Filter<InstitutionRequestDataTransfer>(
                request => request.Status == InstitutionRequestStatus.Pending);

                var institutionRequests =
                    await GetService<IInstitutionService>().GetAllInstitutionRequestsAsync(filter);

                return institutionRequests.Select(institution =>
                    new InstitutionViewModel
                    {
                        Id = institution.Id,
                        Code = institution.InstitutionCode,
                        FullName = institution.InstitutionFullName,
                        ShortName = institution.InstitutionShortName,
                        IsRequest = true
                    }).ToList();
            }

            var institutions =
                await GetService<IInstitutionService>().GetAllInstitutionsAsync();

            return institutions.Select(institution =>
                new InstitutionViewModel
                {
                    Id = institution.Id,
                    Code = institution.Code,
                    FullName = institution.FullName,
                    ShortName = institution.ShortName,
                    IsRequest = false
                }).ToList();
        }

        private async Task<ActionResult> RedirectToLocalAsync(string userId, string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await UserManager.IsInRoleAsync(userId, UserRoles.SystemAdminRole))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (await UserManager.IsInRoleAsync(userId, UserRoles.StudentRole))
            {
                return RedirectToAction("Index", "Timeline");
            }

            if (await UserManager.IsInRoleAsync(userId, UserRoles.InstitutionModeratorRole))
            {
                return RedirectToAction("Index", "Tag");
            }

            return RedirectToAction("Login");
        }

        private async Task<ActionResult> SetInstitutionManager(
            User user,
            RegisterInstitutionManagerViewModel model,
            InstitutionDataTransfer institution,
            bool isNewUser)
        {
            if (!await RoleManager.RoleExistsAsync(UserRoles.InstitutionModeratorRole))
            {
                await RoleManager.CreateAsync(new IdentityRole(UserRoles.InstitutionModeratorRole));
            }

            await UserManager.AddToRoleAsync(user.Id, UserRoles.InstitutionModeratorRole);

            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // ReSharper disable once PossibleNullReferenceException
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { userId = user.Id, code, createPassword = true },
                Request.Url.Scheme);

            StudentDataTransfer student;

            if (isNewUser)
            {
                student = await StudentService.CreateStudentAsync(new StudentDataTransfer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Institution = institution
                });

                await
                    UserManager.SendEmailAsync(
                        user.Id,
                        "Sua conta no CoLearn",
                        $@"<p>Olá {model.FirstName},
                           </p><p><h6>Sua conta como moderador(a) da instituição <b>{institution
                            .FullName}</b> foi criada.</h6></p>
                           <p>Para começar a usá-la, clique <a href=""{callbackUrl}"">aqui</a> para verficar seu e-mail e inserir seus dados.</p>");
            }
            else
            {
                student = await StudentService.GetStudentByEmailAsync(user.Email);

                await
                    UserManager.SendEmailAsync(
                        user.Id,
                        "Sua conta no CoLearn",
                        $@"<p>Olá {model.FirstName},
                           </p><p><h6>Você foi escolhido para ser moderador(a) da instituição <b>{institution
                            .FullName}</b>!</h6></p>
                           <p>Agora você poderá nos ajudar a moderar novas tags pedidas pelos estudantes :)</p>");
            }

            if (student.Institution == null)
            {
                student.Institution = institution;
            }

            student.ModeratingInstitutions.Add(institution);
            student.IsModerator = true;
            await StudentService.UpdateStudentAsync(student);
            return RedirectToAction("Index", "Admin");
        }
    }
}