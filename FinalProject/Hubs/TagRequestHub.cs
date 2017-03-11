using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using FinalProject.Context;
using FinalProject.Models;
using FinalProject.Service;
using FinalProject.Service.Notification;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;

namespace FinalProject.Hubs
{
    public class TagRequestHub : BaseHub
    {
        private ApplicationUserManager UserManager { get; } =
            new ApplicationUserManager(new UserStore<User>(new UserDbContext()));

        public static void NotifyNewTagRequest(TagRequestNotificationData notificationData)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TagRequestHub>();
            var group = context.Clients.Group("inst-" + notificationData.TagRequest.Institution.Id);

            group.tagRequestsUpdated(notificationData.PendingRequestsCount);
        }

        public override async Task OnConnected()
        {
            await base.OnConnected();

            var userId = Context.User.Identity.GetUserId();
            var institutionIds = await GetInstiutionIdsForCurrentStudentAsync(userId);

            if (institutionIds.Count > 0)
            {
                string connectionId;

                if (ConnectedUsers.TryGetValue(userId, out connectionId))
                {
                    foreach (var institutionId in institutionIds)
                    {
                        await Groups.Add(connectionId, "inst-" + institutionId);
                    }
                }
            }
        }

        protected async Task<IList<int>> GetInstiutionIdsForCurrentStudentAsync(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var studentService = AutofacDependencyResolver.Current.GetService<IStudentService>();
            var student = await studentService.GetStudentByEmailAsync(user.Email);

            return student?.ModeratingInstitutions?
                .Select(i => i.Id)
                .ToList();
        }
    }
}