using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Factory;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Service;
using FinalProject.Services;
using FinalProject.Context;
using Microsoft.AspNet.Identity.EntityFramework;
using FinalProject.Models;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using System;

namespace FinalProject.Common
{
    public class Bootstrapper
    {
        private const string AdminPasswordHash = @"AIA+8+QEeQvletNQcXjkWwuJriE/hOhRqvo9NYqUzKto4+7g9vQOMPzSNBst9/bEfQ==";

        public static void Run()
        {
            SetAutofacContainer();
            AutoMapperConfiguration.Configure();
            TypeMapperConfiguration.Configure();
            ConfigureDatabase();
        }

        private static void SetAutofacContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<UnitOfWork>()
                .As<IUnitOfWork>()
                .InstancePerRequest();

            builder.RegisterType<ContextFactory>()
                .As<IContextFactory>()
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(IDataAccess).Assembly)
                .AsImplementedInterfaces()
                .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(IService).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

#if DEBUG
            builder.RegisterType<FakeEmailService>().As<IEmailService>();
#else
            builder.RegisterType<EmailService>().As<IEmailService>();
#endif

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void ConfigureDatabase()
        {
            using (var userContext = UserDbContext.Create())
            {
                var userStore = new UserStore<User>(userContext);
                var userManager = new ApplicationUserManager(userStore);
                var adminEmail = WebConfigurationManager.AppSettings[Shared.Constants.EmailServerLoginConfigName];
                var roleStore = new RoleStore<IdentityRole>(userContext);
                var roleManager = new RoleManager<IdentityRole>(roleStore);
                var user = userManager.FindByEmail(adminEmail);

                if (user == null)
                {
                    user = new User
                    {
                        Email = adminEmail,
                        UserName = adminEmail,
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        PasswordHash = AdminPasswordHash
                    };

                    user = userContext.Users.Add(user);

                    if (!roleManager.RoleExists(UserRoles.SystemAdminRole))
                    {
                        roleManager.Create(new IdentityRole(UserRoles.SystemAdminRole));
                    }

                    if (!roleManager.RoleExists(UserRoles.InstitutionModeratorRole))
                    {
                        roleManager.Create(new IdentityRole(UserRoles.InstitutionModeratorRole));
                    }

                    if (!roleManager.RoleExists(UserRoles.StudentRole))
                    {
                        roleManager.Create(new IdentityRole(UserRoles.StudentRole));
                    }

                    userManager.AddToRole(user.Id, UserRoles.SystemAdminRole);
                    userContext.SaveChanges();
                }
            }
        }
    }
}