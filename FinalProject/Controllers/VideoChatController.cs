using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Helpers;
using FinalProject.Hubs;
using FinalProject.Managers;
using FinalProject.Model;
using FinalProject.Processors;
using FinalProject.Searching;
using FinalProject.Service;
using FinalProject.ViewModels;
using Microsoft.AspNet.Identity;
using WebGrease.Css.Extensions;

namespace FinalProject.Controllers
{
    [Authorize]
    public class VideoChatController : BaseController
    {
        private static readonly string TempStorageFolderPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\";

        private static readonly string VideoChatContentStorageFolderPath = TempStorageFolderPath + @"Chats\";

        private static readonly string AudioFramesStorageFolderPath = VideoChatContentStorageFolderPath + @"{0}\Audio\";

        private static readonly string VideoFramesStorageFolderPath = VideoChatContentStorageFolderPath + @"{0}\Video\";

        private static readonly HttpStatusCodeResult VideoChatNotInitializedErrorResult;

        static VideoChatController()
        {
            VideoChatNotInitializedErrorResult = new HttpStatusCodeResult(
                HttpStatusCode.BadRequest,
                "Video chat is not initialized.");
        }
        
        [HttpPost]
        public async Task<ActionResult> CallParticipants(long id)
        {
            if (!IsChatInitialized(id))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var userId = User.Identity.GetUserId();
            var managerInstance = VideoChatManager.GetInstanceFor(id);
            var participants = managerInstance.Participants
                .Where(p => p != userId)
                .ToSafeReadOnlyCollection();

            await CallParticipantsAsync(id, participants);

            return Json("OK");
        }

        public ActionResult Chat(long id)
        {
            if (!IsChatInitialized(id))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var managerInstance = VideoChatManager.GetInstanceFor(id);
            var presenter = managerInstance.Presenter;

            ViewBag.IsPresenter = presenter == User.Identity.GetUserId();
            ViewBag.ChatId = id.ToString();

            return View();
        }

        [HttpPost]
        public ActionResult EndCall(long chatId, long? duration, string error)
        {
            if (!IsChatInitialized(chatId))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var userId = User.Identity.GetUserId();
            var managerInstance = VideoChatManager.GetInstanceFor(chatId);
            var participants = managerInstance.Participants;

            if (!string.IsNullOrWhiteSpace(error))
            {
                VideoChatHub.CallError(chatId, participants.Where(p => p != userId), error);
                return Json("OK");
            }

            VideoChatHub.EndCall(chatId, participants.Where(p => p != userId));

            if (!duration.HasValue || !managerInstance.AnswerId.HasValue)
            {
                return Json("OK");
            }
            
            Task.Run(async () =>
            {
                var videoChatMediaProcessor = new VideoChatMediaProcessor(chatId, duration.Value);

                await Task.Delay(TimeSpan.FromSeconds(30));
                await videoChatMediaProcessor.ProcessAsync();

                var answer = await AnswerService.OnVideoAnswerProcessed(managerInstance.AnswerId.Value);

                await QuestionSearcher.Default.IndexQuestionAsync(answer.Question.Id, answer.Question.Title);
            });

            return Json("OK");
        }

        [HttpPost]
        public ActionResult EnterVideoChat(long id)
        {
            if (!IsChatInitialized(id))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var userId = User.Identity.GetUserId();
            var managerInstance = VideoChatManager.GetInstanceFor(id);
            var participants = managerInstance.Participants;

            if (!managerInstance.AddParticipant(userId))
            {
                return Json("OK");
            }

            VideoChatHub.InitCall(id, participants.Where(p => p != userId));

            return Json("OK");
        }

        public async Task<ActionResult> NewChat(long? id)
        {
            if (!id.HasValue || !IsChatInitialized(id.Value))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var userId = User.Identity.GetUserId();
            var managerInstance = VideoChatManager.GetInstanceFor(id.Value);
            var particiants = managerInstance.Participants;

            if (particiants == null)
            {
                return View("Error");
            }

            var otherParticipantId = particiants.FirstOrDefault(p => p != userId);

            if (otherParticipantId == null)
            {
                return View("Error");
            }

            var student = await GetStudentByUserIdAsync(otherParticipantId);

            ViewBag.OtherParticipantName = student.FirstName;
            ViewBag.ChatId = id.Value.ToString();

            var chatIdString = id.ToString();
            var videoAnswer = new VideoAnswerDataTransfer
            {
                Author = student,
                CreatedDate = DateTime.UtcNow,
                VideoFilePath = FileSystemHelper.BuildPath(VideoChatContentStorageFolderPath,
                    chatIdString, chatIdString + ".mp4")
            };
            
            videoAnswer = (VideoAnswerDataTransfer)await GetService<IAnswerService>().CreateAnswerAsync(videoAnswer);
            managerInstance.AnswerId = videoAnswer.Id;
            return View();
        }
        
        public async Task<ActionResult> Rate(long id)
        {
            var managerInstance = VideoChatManager.GetInstanceFor(id);
            var particiants = managerInstance.Participants;
            var userId = User.Identity.GetUserId();
            var otherParticipantId = particiants.FirstOrDefault(p => p != userId);
            var student = await GetStudentByUserIdAsync(otherParticipantId);

            return View(new RateVideoChatViewModel
            {
                ChatId = id,
                User = student.FirstName
            });
        }

        [HttpPost]
        public async Task<ActionResult> Rate(long chatId, float rating)
        {
            if (!IsChatInitialized(chatId))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var student = await GetCurrentStudentAsync();
            var managerInstance = VideoChatManager.GetInstanceFor(chatId);
            var questionId = managerInstance.QuestionId;
            var answerId = managerInstance.AnswerId;

            if (!answerId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            var answerService = GetService<IAnswerService>();
            var videoAnswer = (VideoAnswerDataTransfer)await answerService.GetAnswerByIdAsync(answerId.Value);

            if (videoAnswer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            
            if (questionId.HasValue)
            {
                var question = await GetService<IQuestionService>().GetQuestionByIdAsync(questionId.Value);

                if (question == null)
                {
                    return new HttpNotFoundResult("Invalid question id");
                }

                videoAnswer.Question = question;
                videoAnswer = (VideoAnswerDataTransfer)await answerService.UpdateAnswerAsync(videoAnswer);
            }
            
            var answerRating = new AnswerRatingDataTransfer
            {
                Value = rating,
                Author = student,
                Answer = videoAnswer
            };

            await answerService.RateAnswerAsync(answerRating);

            return questionId.HasValue
                ? Json(new { result = "OK" })
                : Json(new { result = "NEW", answerId = videoAnswer.Id });
        }

        public async Task<ActionResult> RefuseCall(long id)
        {
            var manager = VideoChatManager.GetInstanceFor(id);
            var student = await GetCurrentStudentAsync();
            var userId = User.Identity.GetUserId();

            VideoChatHub.RefuseCall(id, student.FirstName, manager.Participants.Where(p => p != userId));
            return Json("OK");
        }

        [HttpPost]
        public async Task<ActionResult> RequestNewVideoChat(int userId, int presenterId, int? questionId)
        {
            var studentService = GetService<IStudentService>();
            var currentUserId = User.Identity.GetUserId();
            var otherStudent = await studentService.GetStudentByIdAsync(userId);
            var presenter = await studentService.GetStudentByIdAsync(presenterId);

            if (otherStudent == null)
            {
                return Json("USER ID NOT FOUND");
            }

            var otherUser = await UserManager.FindByEmailAsync(otherStudent.Email);
            var presenterUser = await UserManager.FindByEmailAsync(presenter.Email);
            var managerInstance = VideoChatManager.GetInstanceFor(0);

            managerInstance.InitVideoChat(new List<string>
            {
                currentUserId,
                otherUser.Id
            });

            if (questionId.HasValue)
            {
                managerInstance.QuestionId = questionId.Value;
            }

            var chatId = managerInstance.ChatId;

            var audioFolder = FileSystemHelper.BuildPath(
                TempStorageFolderPath,
                string.Format(AudioFramesStorageFolderPath, chatId));

            if (!FileSystemHelper.FolderExists(audioFolder))
            {
                FileSystemHelper.CreateFolder(audioFolder);
            }

            var videoFolder = FileSystemHelper.BuildPath(
                TempStorageFolderPath,
                string.Format(VideoFramesStorageFolderPath, chatId));

            if (!FileSystemHelper.FolderExists(videoFolder))
            {
                FileSystemHelper.CreateFolder(videoFolder);
            }

            managerInstance.Presenter = presenterUser.Id;

            return Json(new { chatId });
        }

        [HttpPost]
        public ActionResult PostAudioFrame(long chatId, string frameName)
        {
            if (!IsChatInitialized(chatId))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var audioFolder = FileSystemHelper.BuildPath(
                TempStorageFolderPath,
                string.Format(AudioFramesStorageFolderPath, chatId));

            foreach (string upload in Request.Files)
            {
                var file = Request.Files[upload];

                file?.SaveAs(FileSystemHelper.BuildPath(audioFolder, frameName));
            }

            return Json("OK");
        }

        [HttpPost]
        public ActionResult PostVideoFrame(long chatId, string frameName)
        {
            if (!IsChatInitialized(chatId))
            {
                return VideoChatNotInitializedErrorResult;
            }

            var videoFolder = FileSystemHelper.BuildPath(
                TempStorageFolderPath,
                string.Format(VideoFramesStorageFolderPath, chatId));

            foreach (string upload in Request.Files)
            {
                var file = Request.Files[upload];

                file?.SaveAs(FileSystemHelper.BuildPath(videoFolder, frameName));
            }

            return Json("OK");
        }

        private async Task CallParticipantsAsync(long chatId, IReadOnlyCollection<string> participants)
        {
            var names = new List<string>(participants.Count);

            foreach (var participant in participants)
            {
                var student = await GetStudentByUserIdAsync(participant);

                names.Add(student.FirstName);
            }

            VideoChatHub.CallParticipants(chatId, participants, names);
        }

        private static bool IsChatInitialized(long id)
        {
            return VideoChatManager.IsVideoChatInitialized(id);
        }
    }
}