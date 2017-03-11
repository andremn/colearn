using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FinalProject.Model;
using FinalProject.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FinalProject.Controllers
{
    public class BaseController : Controller
    {
        private ApplicationUserManager _userManager;

        protected BaseController()
        {
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }

            set { _userManager = value; }
        }

        public async Task<StudentDataTransfer> GetCurrentStudentAsync()
        {
            return await GetStudentByUserIdAsync(User.Identity.GetUserId());
        }

        public async Task<StudentDataTransfer> GetStudentByUserIdAsync(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException($"User with id {userId} was not found.");
            }

            var student = await GetService<IStudentService>().GetStudentByEmailAsync(user.Email);

            return student;
        }

        public static T GetService<T>() where T : IService
        {
            return DependencyResolver.Current.GetService<T>();
        }
    }
}