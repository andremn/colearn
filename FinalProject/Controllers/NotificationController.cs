using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.ViewModels;
using Microsoft.AspNet.Identity;

namespace FinalProject.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private static readonly NotificationCollection EmptyNotificationCollection = new NotificationCollection
        {
            Notifications = new List<NotificationViewModel>(0)
        };

        [HttpPost]
        public async Task<ActionResult> GetNotifications()
        {
            var notifications = new List<NotificationViewModel>();
            var count = 0L;

            if (User.IsInRole(UserRoles.SystemAdminRole))
            {
                var sysAdminNotifications =
                    await GetNotificationsForSystemAdminAsync();

                if (sysAdminNotifications != EmptyNotificationCollection)
                {
                    notifications.AddRange(sysAdminNotifications.Notifications);
                    count += sysAdminNotifications.Count;
                }
            }
            else
            {
                var student = await GetCurrentStudentAsync();

                if (User.IsInRole(UserRoles.InstitutionModeratorRole))
                {
                    var moderatorNotifications =
                        await GetNotificationsForModeratorAsync(student);

                    if (moderatorNotifications != EmptyNotificationCollection)
                    {
                        notifications.AddRange(moderatorNotifications.Notifications);
                        count += moderatorNotifications.Count;
                    }
                }

                if (User.IsInRole(UserRoles.StudentRole))
                {
                    var studentNotifications =
                        await GetNotificationsForStudentAsync(student);

                    if (studentNotifications != EmptyNotificationCollection)
                    {
                        notifications.AddRange(studentNotifications.Notifications);
                        count += studentNotifications.Count;
                    }
                }
            }
            
            return new JsonNetResult(new
            {
                count,
                notifications
            });
        }

        private async Task<NotificationCollection> GetNotificationsForSystemAdminAsync()
        {
            var institutionService = GetService<IInstitutionService>();
            var pendingInsitutionRequests = await institutionService
                .GetAllPendingInstitutionRequestsAsync();

            if (pendingInsitutionRequests.Count == 0)
            {
                return EmptyNotificationCollection;
            }

            var count = pendingInsitutionRequests.Count;
            var notificationTitle = pendingInsitutionRequests.Count == 1 
                ? "Você tem uma nova instituição para moderar" 
                : $"Você tem {count} novas instituições para moderar";

            return new NotificationCollection
            {
                Count = count,
                Notifications = new List<NotificationViewModel>(1)
                {
                    new NotificationViewModel
                    {
                        Title = notificationTitle,
                        ActionUrl = "/Admin",
                        Category = NotificationCategories.NewInstitutionRequest,
                        UserId = User.Identity.GetUserId()
                    }
                }
            };
        }

        private async Task<NotificationCollection> GetNotificationsForModeratorAsync(StudentDataTransfer student)
        {
            var tagService = GetService<ITagService>();
            var moderatorInstitutionsIds = student.ModeratingInstitutions?.Select(i => i.Id);
            
            if (moderatorInstitutionsIds == null)
            {
                return EmptyNotificationCollection;
            }

            var count = 0L;

            foreach (var institutionId in moderatorInstitutionsIds)
            {
                count += await tagService.GetTagsPendingCountForInstitutionAsync(institutionId);
            }

            if (count == 0L)
            {
                return EmptyNotificationCollection;
            }

            var notificationTitle = count == 1
                ? "Você tem uma nova tag para moderar"
                : $"Você tem {count} novas tags para moderar";

            return new NotificationCollection
            {
                Count = count,
                Notifications = new List<NotificationViewModel>(1)
                {
                    new NotificationViewModel
                    {
                        Title = notificationTitle,
                        ActionUrl = "/Tag",
                        Category = NotificationCategories.NewTagsRequests,
                        UserId = User.Identity.GetUserId()
                    }
                }
            };
        }

        private async Task<NotificationCollection> GetNotificationsForStudentAsync(StudentDataTransfer student)
        {
            var notifications = new List<NotificationViewModel>();
            var user = await UserManager.FindByEmailAsync(student.Email);
            var answerService = GetService<IAnswerService>();
            var newAnswers = await answerService.GetAllUnseenAnswersForStudentAsync(student.Id, user.LastSeenOnline);

            notifications.AddRange(newAnswers.Select(a => new NotificationViewModel
            {
                Title = "Você tem uma nova resposta para sua pergunta!",
                UserId = user.Id,
                ActionUrl = $"/Question/Details/{a.Question.Id}#a{a.Id}",
                Category = NotificationCategories.QuestionAnswered,
                ObjectId = a.Question.Id.ToString()
            }));

            return new NotificationCollection
            {
                Count = notifications.Count,
                Notifications = notifications
            };
        }

        private class NotificationCollection
        {
            public long Count { get; set; }

            public IList<NotificationViewModel> Notifications { get; set; }
        }
    }
}