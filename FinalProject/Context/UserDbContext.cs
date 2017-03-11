using System.Data.Entity;
using FinalProject.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FinalProject.Context
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext()
            : base("DefaultConnection", false)
        {
        }

        public static UserDbContext Create()
        {
            return new UserDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("User");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityRole>().ToTable("Role");
        }
    }
}