using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.Hubs;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNet.Identity;

namespace FinalProject.Controllers
{
    [Authorize]
    public class InstitutionController : BaseController
    {
        // POST: Institution/DenyInsitutionRegisterRequest
        [HttpPost]
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.InstitutionModeratorRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DenyInstitutionRegisterRequest(DenyInstitutionRequestViewModel model)
        {
            var institutionService = GetService<IInstitutionService>();
            var institutionRequest = await institutionService.GetInstitutionRequestByIdAsync(model.InstitutionRequestId);
            var requestOwner = await GetService<IStudentService>().GetStudentByEmailAsync(institutionRequest.OwnerEmail);
            var emailContent = string.Format(
                Resource.DenyInstitutionRequestEmailMessage,
                institutionRequest.InstitutionFullName,
                model.Reason);

            institutionRequest.Status = InstitutionRequestStatus.Declined;
            await institutionService.UpdateInstitutionRequestAsync(institutionRequest);

            var emailService = GetService<IEmailService>();

            await emailService.SendForStudentAsync(
                    requestOwner,
                    string.Format(Resource.RequestReponseEmailSubject, Resource.ProductName),
                    emailContent);

            return
                Json(
                    new
                    {
                        Success = true,
                        Message = string.Format(Resource.EmailSentTo, model.ManagerEmail),
                        institutionRequest.Id
                    });
        }

        // GET: Institution/Request
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.StudentRole)]
        public ActionResult RequestRegister()
        {
            return View();
        }

        // POST: Institution/Request
        [HttpPost]
        [Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.StudentRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RequestRegister(RegisterInstitutionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await UserManager.IsInRoleAsync(User.Identity.GetUserId(), UserRoles.StudentRole))
            {
                var institutionService = GetService<IInstitutionService>();
                var institutionRequest = new InstitutionRequestDataTransfer
                {
                    InstitutionFullName = model.FullName,
                    InstitutionShortName = model.ShortName,
                    InstitutionCode = model.Code,
                    Status = InstitutionRequestStatus.Pending,
                    CreatedTime = DateTime.Now,
                    OwnerEmail = User.Identity.GetUserName()
                };

                await institutionService.CreateInstitutionRequestAsync(institutionRequest);
                
                var pendingRequests = await institutionService.GetInstitutionsToModerateAsync();

                InstitutionRequestHub.NotifyNewInstitutionRequestCountUpdated(pendingRequests.Count);
                NotificationHub.NotifyClientsNewNotification(new NotificationViewModel
                {
                    Title = "Você tem uma nova instituição para moderar",
                    UserId = User.Identity.GetUserId(),
                    ActionUrl = "/Admin",
                    Category = NotificationCategories.NewInstitutionRequest
                });

                return RedirectToAction("Index", "Timeline");
            }

            var institution = new InstitutionDataTransfer
            {
                Code = model.Code,
                FullName = model.FullName,
                ShortName = model.ShortName
            };

            await GetService<IInstitutionService>().CreateInstitutionAsync(institution);
            return RedirectToAction("Index", "Admin");
        }
    }
}