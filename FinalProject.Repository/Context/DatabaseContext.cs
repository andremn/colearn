using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using FinalProject.Model;
using System;

namespace FinalProject.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<AnswerRating> AnswerRatings { get; set; }

        public DbSet<InstitutionRequest> InstitutionRequests { get; set; }

        public DbSet<Institution> Institutions { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<TagRequest> TagRequests { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public DbSet<Calendar> CalendarFiles { get; set; }

        public DbSet<Preference> Preferences { get; set; }

        public DbSet<Grade> Grades { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Tag>()
                .HasMany(m => m.Children)
                .WithMany(p => p.Parents)
                .Map(w => w.ToTable("TagGraph")
                .MapLeftKey("Parent_Id")
                .MapRightKey("Tag_Id"));

            modelBuilder.Entity<Student>()
                .HasMany(m => m.ModeratingInstitutions)
                .WithMany(p => p.Moderators)
                .Map(w => w.ToTable("InstitutionModerator")
                .MapLeftKey("Student_Id")
                .MapRightKey("Institution_Id"));

            modelBuilder.Entity<Preference>()
                .HasMany(p => p.Institutions)
                .WithMany()
                .Map(w => w.ToTable("PreferenceInstitution")
                .MapLeftKey("Preference_Id")
                .MapRightKey("Institution_Id"));

            modelBuilder.Entity<Student>().Property(student => student.Email).IsRequired();
        }
    }
}