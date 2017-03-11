using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FinalProject.DataAccess.Filters;
using FinalProject.Extensions;
using FinalProject.Helpers;
using FinalProject.Hubs;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Searching;
using FinalProject.Searching.WebCrawlers;
using FinalProject.Searching.WebCrawlers.Specialized;
using FinalProject.Service;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace FinalProject.Controllers
{
    [System.Web.Mvc.Authorize]
    public class QuestionController : BaseController
    {
        private static readonly QuestionSearcher Searcher = QuestionSearcher.Default;

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> AnswerQuestion(NewQuestionAnswerViewModel model)
        {
            var student = await GetCurrentStudentAsync();
            var question = await GetService<IQuestionService>().GetQuestionByIdAsync(model.QuestionId);

            if (question == null)
            {
                return Json(new { success = false });
            }

            var answer = new TextAnswerDataTransfer
            {
                Author = student,
                CreatedDate = DateTime.UtcNow,
                Question = question,
                Text = model.AnswerText
            };

            var answerService = GetService<IAnswerService>();

            answer = (TextAnswerDataTransfer)await answerService.CreateAnswerAsync(answer);

            var user = await UserManager.FindByEmailAsync(question.Author.Email);
            var emailService = GetService<IEmailService>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await NotifyStudentNewAnswerAsync(answer, question, user.Id, emailService));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            
            var data = new
            {
                questionId = question.Id,
                success = true
            };

            var jsonData = JsonConvert.SerializeObject(data);

            QuestionHub.NotifyNewAnswer(jsonData);
            return Json(data);
        }

        // GET: Question/Create
        public ActionResult Create(int? answerId)
        {
            return View(new CreateQuestionViewModel
            {
                AnswerId = answerId
            });
        }

        // POST: Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(CreateQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                throw new InvalidOperationException(
                    "The current user must be an student in order to create a question.");
            }

            var acceptedTags = new List<TagAcceptedDataTransfer>();
            var tagRequests = new List<TagRequestDataTransfer>();
            var tagService = GetService<ITagService>();
            var tagsForInstitution = await tagService.GetTagsByInstitutionIdAsync(student.Institution.Id);
            var parsedTags = model.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in parsedTags)
            {
                var existingTag =
                    tagsForInstitution.SingleOrDefault(
                        t => t.Text.Equals(tag, StringComparison.InvariantCultureIgnoreCase));

                if (existingTag != null)
                {
                    acceptedTags.Add(existingTag);
                }
                else
                {
                    tagRequests.Add(new TagRequestDataTransfer
                    {
                        Text = tag,
                        Institution = student.Institution
                    });
                }
            }

            var needsModeration = tagRequests.Count > 0 && acceptedTags.Count == 0;

            var question = new QuestionDataTransfer
            {
                Institution = student.Institution,
                Author = student,
                CreatedDate = DateTime.UtcNow,
                Description = model.Description,
                Title = model.Title,
                Tags = acceptedTags,
                Status = needsModeration || model.AnswerId.HasValue
                    ? QuestionStatus.PendingApproval
                    : QuestionStatus.Created
            };

            var questionService = GetService<IQuestionService>();

            question = await questionService.CreateQuestionAsync(question);

            string pendingTagsMessage = null;

            if (tagRequests.Count == tagRequests.Count + acceptedTags.Count)
            {
                pendingTagsMessage = Resource.QuestionNoTagAded;
            }
            else if (tagRequests.Count == 1)
            {
                pendingTagsMessage = string.Format(Resource.QuestionTagNotAdded, tagRequests[0].Text);
            }
            else if (tagRequests.Count > 1)
            {
                var pendingTags = new StringBuilder();
                int i;

                for (i = 0; i < tagRequests.Count - 1; i++)
                {
                    pendingTags.Append($"'{tagRequests[i].Text}'");
                }

                pendingTags.Append($" e '{tagRequests[i].Text}'");
                pendingTagsMessage = string.Format(Resource.QuestionTagsNotAdded, pendingTags);
            }

            if (tagRequests.Count > 0)
            {
                foreach (var tagRequest in tagRequests)
                {
                    tagRequest.Question = question;
                    await tagService.CreateTagRequestAsync(tagRequest);
                }

                if (needsModeration)
                {
                    var query = model.Title + " " + string.Join(" ", model.Tags.Split(','));

                    return Json(new { pendingTagsMessage, redirectUrl = "/Question/LearningMaterials?query=" + query });
                }
            }

            if (question.Status == QuestionStatus.Created)
            {
                await Searcher.IndexQuestionAsync(question.Id, question.Title);
            }

            if (!model.AnswerId.HasValue)
            {
                return Json(new { pendingTagsMessage, redirectUrl = "/Timeline/Index" });
            }

            var answerService = GetService<IAnswerService>();
            var videoAnswer = (VideoAnswerDataTransfer)await answerService.GetAnswerByIdAsync(
                model.AnswerId.Value);

            videoAnswer.Question = question;
            await answerService.UpdateAnswerAsync(videoAnswer);

            if (!FileSystemHelper.FileExists(videoAnswer.VideoFilePath))
            {
                return Json(new { pendingTagsMessage, redirectUrl = "/Timeline/Index" });
            }

            question.Status = QuestionStatus.Created;
            await questionService.UpdateQuestionAsync(question);
            await Searcher.IndexQuestionAsync(question.Id, question.Title);

            return Json(new { pendingTagsMessage, redirectUrl = "/Timeline/Index" });
        }
        
        public async Task<ActionResult> GetAnswersViewForQuestion(int id)
        {
            var student = await GetCurrentStudentAsync();
            var answerService = GetService<IAnswerService>();
            var questionService = GetService<IQuestionService>();
            var question = await questionService.GetQuestionByIdAsync(id);
            var answers = await answerService.GetAllAnswersForQuestionAsync(id);

            var answerViewModels = new List<QuestionAnswerViewModel>();

            foreach (var answer in answers)
            {
                var videoAnswer = answer as VideoAnswerDataTransfer;

                if (videoAnswer != null)
                {
                    if (!FileSystemHelper.FileExists(videoAnswer.VideoFilePath))
                    {
                        continue;
                    }
                }

                var answerViewModel = await CreateQuestionAnswerViewModelAsync(
                    answer,
                    student,
                    question);

                answerViewModels.Add(answerViewModel);
            }

            answerViewModels = answerViewModels.OrderByDescending(a => a.Rating).ToList();

            return PartialView("_AnswerListPartial", answerViewModels);
        }

        public async Task<ActionResult> Details(int id)
        {
            var questionService = GetService<IQuestionService>();
            var student = await GetCurrentStudentAsync();
            var question = await questionService.GetQuestionByIdAsync(id);

            var canAnswer = false;

            if (student.InstructorTags != null && student.InstructorTags.Count > 0)
            {
                canAnswer =student.InstructorTags.Any(x => 
                    question.Tags.Any(y => 
                        y.Text.Equals(x.Text, StringComparison.InvariantCultureIgnoreCase)));
            }

            var model = new QuestionDetailsViewModel
            {
                Id = question.Id,
                DateCreated = question.CreatedDate.ToShortDateString(),
                Description = question.Description,
                Tags = question.Tags.Select(t => t.Text).ToList(),
                UserPicture = question.Author.GetProfilePicture(),
                UserName = $"{question.Author.FirstName} {question.Author.LastName}",
                UserId = question.Author.Id,
                Title = question.Title,
                IsOwner = student.Id == question.Author.Id,
                CanAnswer = canAnswer
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> GetSuggestions(string query)
        {
            var answerService = GetService<IAnswerService>();
            var results = await Searcher.SearchSimilarsAsync(query);
            var suggestions = new List<object>(results.Count);

            foreach (var result in results)
            {
                var answersCount = await answerService.GetAnswersCount(new Filter<AnswerDataTransfer>(a => result.Id == a.Question.Id));
                var answersCountText = answersCount > 0
                    ? (answersCount > 1 ? answersCount + " respostas" : "1 resposta")
                    : "Nenhuma resposta";

                suggestions.Add(new
                {
                    id = result.Id,
                    title = result.Title,
                    answers = answersCountText
                });
            }

            return Json(new { suggestions });
        }

        public ActionResult LearningMaterials(string query)
        {
            return string.IsNullOrWhiteSpace(query)
                ? View("Error")
                : View(new LearningMaterialsViewModel { Query = query });
        }

        [HttpPost]
        public async Task<ActionResult> LoadMaterials(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return PartialView("_LearningMaterialsPartial");
            }

            var keywords = await Searcher.ExtractKeywordsAsync(query);
            var crawler = MerlotCrawler.Instance;
            var materials = await crawler.GetResultAsync(keywords);
            
            return GetPartialViewForMaterials(materials, 1);
        }
        
        public async Task<ActionResult> GetMaterials(uint page)
        {
            var crawler = MerlotCrawler.Instance;
            var materials = await crawler.GetResultForPageAsync(page);

            return GetPartialViewForMaterials(materials, page);
        }

        [HttpPost]
        public async Task<ActionResult> RateAnswer(AnswerRatingViewModel model)
        {
            var answerService = GetService<IAnswerService>();
            var answer = await answerService.GetAnswerByIdAsync(model.Id);

            if (answer == null)
            {
                return Json(new { success = false });
            }

            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return Json(new { success = false });
            }

            if (answer.Ratings.Any(r => r.Author.Id == student.Id))
            {
                return Json(new { success = false, error = "ALREADY_RATED" });
            }

            var rating = new AnswerRatingDataTransfer
            {
                Value = model.Value,
                Answer = answer,
                Author = student
            };

            await answerService.RateAnswerAsync(rating);

            var average = await answerService.GetAvarageRatingByIdAsync(answer.Id);
            
            return Json(new { success = true, rating = average });
        }

        [HttpPost]
        public async Task<ActionResult> StartVideoCall(VideoAnswerViewModel callDetails)
        {
            var student = await GetService<IStudentService>().GetStudentByIdAsync(callDetails.StudentId);
            var userManager = Request.GetOwinContext().Get<ApplicationUserManager>();
            var user = await userManager.FindByEmailAsync(student.Email);
            string connectionId;

            if (!BaseHub.ConnectedUsers.TryGetValue(user.Id, out connectionId))
            {
                return Json(new { success = false, error = "Estudante não conectado" });
            }

            var hub = GlobalHost.ConnectionManager.GetHubContext<WebRtcHub>();
            var client = hub.Clients.Client(connectionId);

            client.newCall(callDetails.PeerConnection);
            return Json(new { success = true });
        }

        private ActionResult GetPartialViewForMaterials(IEnumerable<LearningMaterial> materials, uint pageNumber)
        {
            var materialsViewModel = new LearningMaterialsViewModel
            {
                Pages = MerlotCrawler.Instance.PageNumbers,
                CurrentPage = pageNumber
            };

            var materialList = materials.Select(material =>
                new LearningMaterialViewModel
                {
                    Title = material.Title,
                    Link = material.Link,
                    Source = material.Source
                }).ToList();

            materialsViewModel.Materials = materialList;

            return PartialView("_LearningMaterialsPartial", materialsViewModel);
        }

        private static async Task<QuestionAnswerViewModel> CreateQuestionAnswerViewModelAsync(
            AnswerDataTransfer answer,
            StudentDataTransfer currentStudent,
            QuestionDataTransfer question)
        {
            var answerService = GetService<IAnswerService>();
            var studentService = GetService<IStudentService>();
            var answerAuthor = await studentService.GetStudentByIdAsync(answer.Author.Id);
            var averageRating = await answerService.GetAvarageRatingByIdAsync(answer.Id);
            var isVideoAnswer = answer is VideoAnswerDataTransfer;
            var hasUserRatedAnswer = answer.Ratings.Any(r => r.Author.Id == currentStudent.Id);
            var canUserRateAnswer = currentStudent.InstructorTags
                .Any(instructorTag => question.Tags.Any(t => instructorTag.Id == t.Id));

            var model = new QuestionAnswerViewModel
            {
                Id = answer.Id,
                DateCreated = answer.CreatedDate.ToLocalTime().ToShortDateString(),
                UserPicture = answer.Author.GetProfilePicture(),
                UserName = $"{answerAuthor.FirstName} {answerAuthor.LastName}",
                UserId = answerAuthor.Id,
                CanRate = !hasUserRatedAnswer && canUserRateAnswer,
                IsOwner = answerAuthor.Id == currentStudent.Id,
                Count = answer.Ratings.Count,
                QuestionId = question.Id,
                IsVideoAnswer = isVideoAnswer,
                Rating = float.IsNaN(averageRating)
                    ? null
                    : (float?)averageRating
            };

            var videoAnswer = answer as VideoAnswerDataTransfer;

            if (videoAnswer != null)
            {
                var chatId = Path.GetFileNameWithoutExtension(videoAnswer.VideoFilePath);

                model.Content = $"/api/videos/{chatId}";
                model.Thumbnail = $"/VideoChatThumbs/{chatId}.png";
            }
            else
            {
                model.Content = ((TextAnswerDataTransfer)answer).Text;
            }

            return model;
        }

        private static async Task NotifyStudentNewAnswerAsync(
            TextAnswerDataTransfer answer,
            QuestionDataTransfer affectedQuestion,
            string userId,
            IEmailService emailService)
        {
            try
            {
                var notificationModel = new NotificationViewModel
                {
                    ActionUrl = $"/Question/Details/{affectedQuestion.Id}/#{answer.Id}",
                    Title = Resource.NewAnswerToQuestionNotificationTitle,
                    UserId = userId,
                    Category = NotificationCategories.QuestionAnswered,
                    ObjectId = answer.Question.Id.ToString()
                };

                NotificationHub.NotifyClientsNewNotification(notificationModel);
                QuestionHub.NotifyClientNewAnswer(userId);

                var student = affectedQuestion.Author;

                var emailContent = string.Format(
                    Resource.NewAnswerToQuestionEmailContent,
                    answer.Question.Title,
                    answer.Text);

                await emailService.SendForStudentAsync(
                        student,
                        Resource.NewAnswerToQuestionEmailSubject,
                        emailContent);
            }
            catch
            {
                // Ignored
            }
        }
    }
}