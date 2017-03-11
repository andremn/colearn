using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.DataAccess.Filters;
using FinalProject.Extensions;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.Service.Admin;
using FinalProject.ViewModels;

namespace FinalProject.Controllers
{
    public class SearchController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var institutions = await GetService<IInstitutionService>().GetAllInstitutionsAsync();
            var institutionViewModels = institutions.Select(i => new InstitutionSelectItemViewModel
            {
                Id = i.Id,
                Code = i.Code,
                FullName = i.FullName,
                ShortName = i.ShortName
            }).ToList();

            var grades = await GetService<IGradeService>().GetAllGradesAsync();
            var gradeViewModels = grades.Select(g => new GradeListItemViewModel
            {
                Id = g.Id,
                Name = g.Name
            }).ToList();
            
            return View(new StudentSearchViewModel
            {
                SelectInstitutions = institutionViewModels,
                SelectGrades = gradeViewModels
            });
        }
        
        public async Task<ActionResult> GetRecommendedInstructors(StudentSearchViewModel model)
        {
            var tagComparer = new TagAcceptedDataTransferEqualityComparer();
            var recommendationService = GetService<IRecommendationService>();
            var questionService = GetService<IQuestionService>();
            var student = await GetCurrentStudentAsync();
            var studentQuestions = await questionService
                .GetAllQuestionsForStudentAsync(student.Id, null, int.MaxValue);
            var students = await recommendationService
                .GetRecommendedInstructorsForStudentAsync(student);
            var studentTags = studentQuestions
                .SelectMany(q => q.Tags)
                .Distinct(tagComparer);

            Func<TagAcceptedDataTransfer, TagViewModel> toTagViewModelFunc =
                t => new TagViewModel
                {
                    Id = t.Id,
                    Text = t.Text
                };

            var recommendedInstructors = students.Select(s => new StudentSearchListItemViewModel
            {
                Id = s.Id,
                Email = s.Email,
                Institution = s.Institution.ShortName,
                ProfilePic = s.GetProfilePicture(),
                Name = $"{s.FirstName} {s.LastName}",
                Similarity = (s.Similarity * 100).ToString("#.##", CultureInfo.InvariantCulture),
                AvgRating = s.AvgRating.ToString(CultureInfo.InvariantCulture),
                RatingsCount = s.RatingsCount,
                InstructorTags = s.InstructorTags?.Intersect(studentTags, tagComparer).Select(toTagViewModelFunc).ToList(),
                Grade = s.Grade.ToString()
            }).ToList();

            var searchListViewModel = new StudentSearchListViewModel
            {
                Students = recommendedInstructors
            };

            return PartialView("_RecommendedInstructorsPartial", searchListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SearchStudents(StudentSearchViewModel model)
        {
            var filters = new List<Expression<Func<StudentDataTransfer, bool>>>();

            if (!string.IsNullOrWhiteSpace(model.StudentName))
            {
                var nameFilters = new List<Expression<Func<StudentDataTransfer, bool>>>
                {
                    s => s.FirstName.Contains(model.StudentName),
                    s => s.LastName.Contains(model.StudentName)
                };

                filters.Add(nameFilters.Or());
            }

            if (model.Grades != null && model.Grades.Count > 0)
            {
                var gradeFilters = model.Grades
                    .Select(id => ( Expression<Func<StudentDataTransfer, bool>> )
                        (s => s.Grade.Id == id));

                filters.Add(gradeFilters.Or());
            }

            if (model.Institutions != null && model.Institutions.Count > 0)
            {
                var institutionFilters = model.Institutions
                    .Select(id => ( Expression<Func<StudentDataTransfer, bool>> )
                        (s => s.Institution.Id == id));

                filters.Add(institutionFilters.Or());
            }

            Filter<StudentDataTransfer> filter = null;

            if (filters.Count > 0)
            {
                filter = new Filter<StudentDataTransfer>(filters.And());
            }
            
            var studentService = GetService<IStudentService>();
            var students = await studentService.GetAllStudentsAsync(filter);
            
            if (model.StudentMinAvgRating > 0f)
            {
                students = students
                    .Where(s => s.AvgRating >= model.StudentMinAvgRating)
                    .ToList();
            }

            Func<TagAcceptedDataTransfer, TagViewModel> toTagViewModelFunc =
                t => new TagViewModel
                {
                    Id = t.Id,
                    Text = t.Text
                };

            var studentsListViewModel = students.Select(s => new StudentSearchListItemViewModel
            {
                Id = s.Id,
                Email = s.Email,
                Institution = s.Institution.ShortName,
                ProfilePic = s.GetProfilePicture(),
                Name = $"{s.FirstName} {s.LastName}",
                AvgRating = s.AvgRating.ToString(CultureInfo.InvariantCulture),
                InstructorTags = s.InstructorTags?.Select(toTagViewModelFunc).Take(3).ToList(),
                QuestionTags = s.QuestionTags?.Select(toTagViewModelFunc).Take(3).ToList(),
                Grade = s.Grade.ToString()
            }).OrderByDescending(s => s.AvgRating).ToList();

            var searchListViewModel = new StudentSearchListViewModel
            {
                Students = studentsListViewModel
            };

            var htmlView = this.RenderViewToString(
                "_SearchListPartial",
                searchListViewModel);

            return Json(htmlView);
        }

        private class TagAcceptedDataTransferEqualityComparer : IEqualityComparer<TagAcceptedDataTransfer>
        {
            public bool Equals(TagAcceptedDataTransfer x, TagAcceptedDataTransfer y)
            {
                return x.Text.Equals(y.Text, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(TagAcceptedDataTransfer obj)
            {
                return obj.Text.GetHashCode();
            }
        }
    }
}