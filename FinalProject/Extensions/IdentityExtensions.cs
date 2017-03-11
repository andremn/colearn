using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using FinalProject.Common;
using FinalProject.Model;
using FinalProject.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Nito.AsyncEx;

namespace FinalProject.Extensions
{
    public static class IdentityExtensions
    {
        public static InstitutionDataTransfer GetInstitution(this IIdentity identity, IOwinContext context)
        {
            if (identity == null)
            {
                return null;
            }

            var userManager = context.GetUserManager<ApplicationUserManager>();
            var userId = identity.GetUserId();

            if (userManager.IsInRole(userId, UserRoles.StudentRole)
                || userManager.IsInRole(userId, UserRoles.InstitutionModeratorRole))
            {
                var student = AsyncContext.Run(async () => await GetStudentAsync(identity));

                return student?.Institution;
            }

            throw new ArgumentException(
                $"Identity must in role '{UserRoles.StudentRole}' or '{UserRoles.InstitutionModeratorRole}'");
        }

        public static string GetName(this IIdentity identity, IOwinContext context)
        {
            if (identity == null)
            {
                return null;
            }

            var userManager = context.GetUserManager<ApplicationUserManager>();

            if (userManager.IsInRole(identity.GetUserId(), UserRoles.StudentRole)
                || userManager.IsInRole(identity.GetUserId(), UserRoles.InstitutionModeratorRole))
            {
                return AsyncContext.Run(async () => await GetStudentAsync(identity)).FirstName;
            }

            if (userManager.IsInRole(identity.GetUserId(), UserRoles.SystemAdminRole))
            {
                return "Administrador";
            }

            return string.Empty;
        }

        public static string GetProfilePicture(this IIdentity identity)
        {
            var student = AsyncContext.Run(async () => await GetStudentAsync(identity));

            return student.GetProfilePicture();
        }

        public static IEnumerable<string> GetRoles(this IIdentity identity, IOwinContext context)
        {
            var userManager = context.GetUserManager<ApplicationUserManager>();

            return userManager.GetRoles(identity.GetUserId());
        }

        public static async Task<DateTime?> GetLastSeenOnlineDateTime(
            this IIdentity identity,
            ApplicationUserManager userManager)
        {
            var userId = identity.GetUserId();
            var user = await userManager.FindByIdAsync(userId);

            return user?.LastSeenOnline;
        }

        public static async Task<StudentDataTransfer> GetStudentAsync(this IIdentity identity)
        {
            var studentService = AutofacDependencyResolver.Current.GetService<IStudentService>();

            return await studentService.GetStudentByEmailAsync(identity.GetUserName());
        }
    }
}