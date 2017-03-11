using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.Extensions;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.Service.Admin;
using FinalProject.ViewModels;

namespace FinalProject.Controllers
{
    [Authorize(Roles = UserRoles.SystemAdminRole)]
    public class AdminController : BaseController
    {
        // GET: Admin
        public async Task<ActionResult> Index()
        {
            var institutions = await GetService<IInstitutionService>().GetInstitutionsToModerateAsync();
            var institutionModels = institutions
                .Select(i => new InstitutionViewModel
                {
                    Id = i.Id,
                    Code = i.InstitutionCode,
                    FullName = i.InstitutionFullName,
                    ShortName = i.InstitutionShortName,
                    RequesterEmail = i.OwnerEmail,
                    IsRequest = true
                }).ToList();

            return View(new RegisterInstitutionManagerViewModel
            {
                Institutions = institutionModels
            });
        }

        // POST: Admin/SetResult
        [HttpPost]
        public async Task<ActionResult> SetInstitutionStatus(int id, InstitutionRequestStatus status)
        {
            var institutionService = GetService<IInstitutionService>();

            try
            {
                var institutionRequest = await institutionService.GetInstitutionRequestByIdAsync(id);

                institutionRequest.Status = status;
                institutionRequest = await institutionService.UpdateInstitutionRequestAsync(institutionRequest);
                return Json(new {Success = institutionRequest.Status == status});
            }
            catch
            {
                return Json(new {Success = false});
            }
        }

        public async Task<ActionResult> ManageGrades()
        {
            var grades = await GetService<IGradeService>().GetAllGradesAsync();

            var model = new ManageGradesViewModel
            {
                Grades = grades.Select(g => new GradeListItemViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Order = g.Order
                }).OrderBy(g => g.Order).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateGrade(GradeListItemViewModel model)
        {
            var gradeDto = new GradeDataTransfer
            {
                Id = model.Id,
                Name = model.Name,
                Order = model.Order
            };

            gradeDto = await GetService<IGradeService>().UpdateGradeAsync(gradeDto);
            
            return Json(new
            {
                id = gradeDto.Id,
                name = gradeDto.Name,
                order = gradeDto.Order
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreteGrade(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var grade = new GradeDataTransfer
            {
                Name = name
            };

            grade = await GetService<IGradeService>().CreateGradeAsync(grade);

            var itemModel = new GradeListItemViewModel
            {
                Id = grade.Id,
                Name = grade.Name
            };

            return Json(this.RenderViewToString("_GradeListITemPartial", itemModel));
        }
    }
}