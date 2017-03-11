using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.Service.Admin;
using FinalProject.ViewModels;

namespace FinalProject.Controllers
{
    [Authorize]
    public class PreferenceController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var preferenceService = GetService<IPreferenceService>();
            PreferenceDataTransfer preference;

            if (!User.IsInRole(UserRoles.SystemAdminRole))
            {
                var student = await GetCurrentStudentAsync();

                preference = await preferenceService.GetPreferenceByStudentId(student.Id);
            }
            else
            {
                preference = await preferenceService.GetDefaultPreferenceAsync();
            }

            var institutions = await GetService<IInstitutionService>().GetAllInstitutionsAsync();
            var institutionViewModels = institutions.Select(i => new InstitutionSelectItemViewModel
            {
                Id = i.Id,
                Code = i.Code,
                FullName = i.FullName,
                ShortName = i.ShortName,
                IsSelected = preference.Institutions?.Any(x => x.Id == i.Id) ?? false
            }).ToArray();

            var grades = await GetService<IGradeService>().GetAllGradesAsync();
            var gradeViewModels = grades.Select(i => new GradeListItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                IsSelected = preference.Grade != null && 
                    preference.Grade.Id == i.Id
            }).ToArray();
            
            var model = new PreferencesViewModel
            {
                Id = preference.Id,
                SelectInstitutions = institutionViewModels,
                SelectGrades = gradeViewModels,
                MinSimilarity = preference.MinSimilarity,
                MinRating = preference.MinRating
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(PreferencesViewModel model)
        {
            var preferenceService = GetService<IPreferenceService>();
            var preference = new PreferenceDataTransfer
            {
                Id = model.Id,
                MinRating = model.MinRating,
                MinSimilarity = model.MinSimilarity,
                Institutions = model.Institutions?.Select(i => new InstitutionDataTransfer
                {
                    Id = int.Parse(i)
                }).ToList() ?? new List<InstitutionDataTransfer>(0)
            };

            if (!User.IsInRole(UserRoles.SystemAdminRole))
            {
                var student = await GetCurrentStudentAsync();

                preference.Student = student;

                if (model.Grade.HasValue)
                {
                    preference.Grade = new GradeDataTransfer { Id = model.Grade.Value };
                }
            }

            preference = await preferenceService.UpdatePreferenceAsync(preference);

            return Json(new
            {
                success = true,
                preferences = preference
            });
        }

        [HttpPost]
        public async Task<ActionResult> Reset()
        {
            var student = await GetCurrentStudentAsync();
            var preferenceService = GetService<IPreferenceService>();
            var preference = await preferenceService.ResetPrefereceForStudentAsync(student.Id);

            return Json(new
            {
                success = true,
                preferences = preference
            });
        }
    }
}