using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IStudentDataAccess : IDataAccess<int, Student>
    {
        Task<Student> GetByEmailAsync(string email);

        Task<IList<Tag>> GetQuestionsTagsAsync(int studentId);

        Task<float> GetAverageRatingAsync(int studentId);
    }

    internal class StudentDataAccess : IStudentDataAccess
    {
        public async Task<long> CountAsync(IFilter<Student> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Students.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<Student> CreateAsync(Student item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Institution != null)
                {
                    context.Institutions.Attach(item.Institution);
                }

                if (item.ModeratingInstitutions != null)
                {
                    var institutions = new List<Institution>(item.ModeratingInstitutions);

                    item.ModeratingInstitutions.Clear();

                    foreach (var institution in institutions)
                    {
                        var existingInstitution = await context.Institutions.FindAsync(institution.Id);

                        if (existingInstitution == null)
                        {
                            continue;
                        }

                        context.Entry(existingInstitution).CurrentValues.SetValues(institution);
                        item.ModeratingInstitutions.Add(existingInstitution);
                    }
                }

                if (item.InstructorTags != null)
                {
                    var tags = new List<Tag>(item.InstructorTags);

                    item.InstructorTags.Clear();

                    foreach (var tag in tags)
                    {
                        var existingTag = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == tag.Id);

                        if (existingTag == null)
                        {
                            continue;
                        }

                        context.Entry(existingTag).CurrentValues.SetValues(tag);
                        item.InstructorTags.Add(existingTag);
                    }
                }

                context.Grades.Attach(item.Grade);

                var createdStudent = context.Students.Add(item);

                await context.SaveChangesAsync();
                return createdStudent;
            }
        }

        public async Task<Student> DeleteAsync(Student item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<Student> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var student =
                    await
                        context.Students.Include(s => s.ModeratingInstitutions)
                            .Include(s => s.Institution)
                            .Include(s => s.InstructorTags)
                            .SingleOrDefaultAsync(s => s.Id == id);

                student = context.Students.Remove(student);
                await context.SaveChangesAsync();
                return student;
            }
        }

        public async Task<IList<Student>> GetAllAsync(IFilter<Student> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Students.
                    Include(s => s.ModeratingInstitutions)
                    .Include(s => s.Institution)
                    .Include(s => s.Grade)
                    .Include(s => s.InstructorTags)
                    .AsNoTracking();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<Student> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(nameof(email));
            }

            using (var context = new DatabaseContext())
            {
                return await context.Students
                    .Include(s => s.ModeratingInstitutions)
                    .Include(s => s.Institution)
                    .Include(s => s.InstructorTags)
                    .Include(s => s.Grade)
                    .Where(student => student.Email == email)
                    .AsNoTracking()
                    .SingleOrDefaultAsync();
            }
        }

        public async Task<IList<Tag>> GetQuestionsTagsAsync(int studentId)
        {
            using (var context = new DatabaseContext())
            {
                var questions = context.Questions
                    .Include(q => q.Author)
                    .Include(q => q.Tags)
                    .Where(q => q.Author.Id == studentId)
                    .AsNoTracking();

                var tags = await questions
                    .SelectMany(q => q.Tags)
                    .AsNoTracking()
                    .OrderByDescending(t => t.Questions.Count)
                    .ToListAsync();

                return tags
                    .Distinct(new TagDataAccess.TagEqualityComparer())
                    .ToList();
            }
        }

        public async Task<float> GetAverageRatingAsync(int studentId)
        {
            using (var context = new DatabaseContext())
            {
                var answers = context.Answers
                    .Include(a => a.Author)
                    .Include(a => a.Ratings)
                    .Where(q => q.Author.Id == studentId)
                    .AsNoTracking();

                var answersList = await answers.ToListAsync();

                if (answersList.Count == 0)
                {
                    return 0f;
                }

                var ratedAnswers = answersList.Where(a => a.Ratings.Count > 0).ToList();
                var sum = ratedAnswers
                    .Aggregate(0f, (current, answer) =>
                        current + answer.Ratings.Sum(r => r.Value) / answer.Ratings.Count);

                return sum / ratedAnswers.Count;
            }
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return await context.Students.Include(s => s.ModeratingInstitutions)
                    .Include(s => s.Institution)
                    .Include(s => s.InstructorTags)
                    .Include(s => s.Grade)
                    .SingleOrDefaultAsync(student => student.Id == id);
            }
        }

        public async Task<Student> UpdateAsync(Student item)
        {
            using (var context = new DatabaseContext())
            {
                var student = await context.Students
                    .Include(s => s.ModeratingInstitutions)
                    .Include(s => s.Institution)
                    .Include(s => s.InstructorTags)
                    .Include(s => s.Grade)
                    .SingleOrDefaultAsync(s => s.Id == item.Id);

                if (item.Grade == null)
                {
                    student.Grade = null;
                }
                else if (student.Grade == null ||
                    item.Grade.Id != student.Grade.Id)
                {
                    context.Grades.Attach(item.Grade);
                    student.Grade = item.Grade;
                }
                
                context.Entry(student).CurrentValues.SetValues(item);

                if (item.Institution != null)
                {
                    student.Institution = await context.Institutions.FindAsync(item.Institution.Id);
                }

                if (item.ModeratingInstitutions != null)
                {
                    student.ModeratingInstitutions.Clear();

                    foreach (var institution in item.ModeratingInstitutions)
                    {
                        var existingInstitution = await context.Institutions.FindAsync(institution.Id);

                        if (existingInstitution == null)
                        {
                            continue;
                        }

                        context.Entry(existingInstitution).CurrentValues.SetValues(institution);
                        student.ModeratingInstitutions.Add(existingInstitution);
                    }
                }

                if (item.InstructorTags != null)
                {
                    student.InstructorTags.Clear();

                    foreach (var tag in item.InstructorTags)
                    {
                        var existingTag = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == tag.Id);

                        if (existingTag != null)
                        {
                            context.Entry(existingTag).CurrentValues.SetValues(tag);
                            student.InstructorTags.Add(existingTag);
                        }
                        else
                        {
                            context.TagRequests.Add(
                                new TagRequest { Institution = student.Institution, Text = tag.Text, Author = student });
                        }
                    }
                }

                await context.SaveChangesAsync();
                return student;
            }
        }
    }
}