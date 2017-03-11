using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.DataAccess.Filters;
using FinalProject.Extensions;
using FinalProject.Helpers;
using FinalProject.Hubs;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.Service.Notification;
using FinalProject.ViewModels;

namespace FinalProject.Controllers
{
    [Authorize]
    public class TimelineController : BaseController
    {
        private const int DefaultMaxItemsToLoad = int.MaxValue;

        static TimelineController()
        {
            NotificationListener.Instance.AddListenerForCategory(
                NotificationCategories.QuestionCreated,
                OnNewQuestionCreated);
        }

        // GET: Timeline
        public async Task<ActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student.Institution == null)
            {
                ViewBag.UserHasInstitution = false;
                return View(new TimelineFilterViewModel { UserId = student.Id });
            }

            var tags = await GetService<ITagService>().GetAllTagsForInstitutionAsync(student.Institution.Id);
            var operators = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = Resource.AndOperator.ToLower(),
                    Selected = true
                },
                new SelectListItem
                {
                    Text = Resource.OrOperator.ToLower()
                }
            };

            ViewBag.UserHasInstitution = true;
            ViewBag.InstitutionId = student.Institution.Id;

            return View(new TimelineFilterViewModel
            {
                UserId = student.Id,
                Operators = operators,
                TagsList = tags
                    .Select(t => t.Text)
                    .Where(t => !t.Contains("root_"))
                    .ToList()
            });
        }

        // POST: Timeline/LoadItems
        [HttpPost]
        public async Task<ActionResult> LoadItems(TimelineFilterViewModel model)
        {
            IFilter<QuestionDataTransfer> filter;

            if (model.Tags?.Length > 0 ||
                !string.IsNullOrWhiteSpace(model.Title))
            {
                filter = TimelineHelper.CreateFilter(model);
            }
            else
            {
                filter = new Filter<QuestionDataTransfer>();
            }

            if (model.LastId.HasValue)
            {
                filter.AddExpression(q => q.Id < model.LastId.Value);
            }

            return await GetJsonItemsAsync(
                filter,
                model);
        }

        // POST: Timeline/LoadPageForItem
        [HttpPost]
        public ActionResult LoadPageForItem(TimelineItemViewModel model)
        {
            return PartialView("_TimelineItemPartial", model);
        }

        private async Task<JsonResult> GetJsonItemsAsync(
            IFilter<QuestionDataTransfer> filter,
            TimelineFilterViewModel model)
        {
            var filterText = TimelineHelper.GetFilterText(model);

            if (model.ShowOnlyStudentInstitutionItems)
            {
                model.UserId = null;
            }
            else if (model.ShowOnlyStudentItems)
            {
                var student = await GetCurrentStudentAsync();

                model.UserId = student.Id;
            }

            var items = await LoadItemsAsync(
                filter,
                model.ItemsType,
                model.MaxItems,
                model.UserId)
                ?? new List<TimelineItemViewModel>();

            var hasMore = model.MaxItems.HasValue
                ? items.Count == model.MaxItems
                : items.Count == DefaultMaxItemsToLoad;

            return Json(new
            {
                hasMore = false,
                filterText,
                items = items.Select(item =>
                    this.RenderViewToString("_TimelineItemPartial", item))
                    .ToList()
            });
        }

        private async Task<IList<TimelineItemViewModel>> GetItemsForUserAsync(
            int studentId,
            IFilter<QuestionDataTransfer> filter,
            int? maxItems,
            ItemsType type)
        {
            var questionService = GetService<IQuestionService>();
            IList<QuestionDataTransfer> questions;

            switch (type)
            {
                case ItemsType.Related:
                    var studentService = GetService<IStudentService>();
                    var student = await studentService.GetStudentByIdAsync(studentId);

                    questions = await questionService.GetAllQuestionsRelatedForStudentAsync(
                        student,
                        filter,
                        maxItems ?? DefaultMaxItemsToLoad);
                    break;
                case ItemsType.Contribution:
                    questions = await questionService.GetAllContributedQuestionsForStudentAsync(
                        studentId,
                        filter,
                        maxItems ?? DefaultMaxItemsToLoad);
                    break;
                case ItemsType.All:
                default:
                    questions = await questionService.GetAllQuestionsForStudentAsync(
                        studentId,
                        filter,
                        maxItems ?? DefaultMaxItemsToLoad);
                    break;
            }

            return questions.Select(TimelineHelper.ConvertToTimelineItem).ToList();
        }

        private async Task<IList<TimelineItemViewModel>> LoadItemsAsync(
            IFilter<QuestionDataTransfer> filter,
            ItemsType type,
            int? maxItems,
            int? userId)
        {
            if (userId.HasValue)
            {
                return await GetItemsForUserAsync(userId.Value, filter, maxItems, type);
            }

            return await GetItemsForInstitutionAsync(filter, maxItems);
        }

        private async Task<IList<TimelineItemViewModel>> GetItemsForInstitutionAsync(IFilter<QuestionDataTransfer> filter,
           int? maxItems)
        {
            var student = await GetCurrentStudentAsync();

            if (student.Institution == null)
            {
                return null;
            }

            var questions = await GetService<IQuestionService>().GetAllQuestionsForInstitutionAsync(
                student.Institution.Id,
                filter,
                maxItems ?? DefaultMaxItemsToLoad);

            return questions.Select(TimelineHelper.ConvertToTimelineItem).ToList();
        }

        public static void OnNewQuestionCreated(IListener sender, NotificationEvent notificationEvent)
        {
            var notificationData = notificationEvent.Data as NewQuestionNotificationData;

            if (notificationData == null)
            {
                return;
            }

            var question = notificationData.Question;
            var item = TimelineHelper.ConvertToTimelineItem(question);

            TimelineHub.NotifyNewQuestionCreated(item);
        }
    }
}