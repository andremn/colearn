using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IQuestionDataAccess : IDataAccess<int, Question>
    {
        Task<IList<Question>> GetAllForInstitutionAsync(int institutionId, int maxItems = 10);

        Task<IList<Question>> GetAllForInstitutionAsync(
            int institutionId,
            IFilter<Question> filter,
            int maxItems = 10);

        Task<IList<Question>> GetAllForStudentAsync(int userId, int maxItems = 10);

        Task<IList<Question>> GetAllForStudentAsync(
            int userId,
            IFilter<Question> filter,
            int maxItems = 10);
    }

    public class QuestionDataAccess : IQuestionDataAccess
    {
        public async Task<long> CountAsync(IFilter<Question> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Questions.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<Question> CreateAsync(Question item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Institution == null || item.Author == null || item.Tags == null)
                {
                    throw new ArgumentException(
                        $@"The fields '{nameof(item.Institution)}', 
                                                '{nameof
                            (item.Author)}' and '{nameof(item.Tags)}' 
                                                are required");
                }

                context.Institutions.Attach(item.Institution);
                context.Students.Attach(item.Author);

                var tags = new List<Tag>(item.Tags);

                item.Tags.Clear();

                foreach (var tag in tags)
                {
                    var existingTag = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == tag.Id);

                    if (existingTag != null)
                    {
                        context.Entry(existingTag).CurrentValues.SetValues(tag);
                        item.Tags.Add(existingTag);
                    }
                }

                var question = context.Questions.Add(item);

                await context.SaveChangesAsync();
                return question;
            }
        }

        public async Task<Question> DeleteAsync(Question item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<Question> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var question =
                    await
                        context.Questions.Include(q => q.Tags)
                            .Include(q => q.Author)
                            .Include(q => q.Institution)
                            .Include(q => q.Answers)
                            .SingleOrDefaultAsync(institution => institution.Id == id);

                question = context.Questions.Remove(question);
                await context.SaveChangesAsync();
                return question;
            }
        }

        public async Task<IList<Question>> GetAllAsync(IFilter<Question> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Questions.Include(q => q.Tags)
                    .Include(q => q.Author)
                    .Include(q => q.Institution)
                    .Include(q => q.Answers);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable
                    .OrderByDescending(q => q.CreatedDate)
                    .ToListAsync();
            }
        }

        public async Task<IList<Question>> GetAllForInstitutionAsync(int institutionId, int maxItems = 10)
        {
            return await GetAllForInstitutionAsync(
                institutionId,
                null,
                maxItems);
        }

        public async Task<IList<Question>> GetAllForInstitutionAsync(
            int institutionId,
            IFilter<Question> filter,
            int maxItems = 10)
        {
            using (var context = new DatabaseContext())
            {
                var queryable =
                    context.Questions.Include(q => q.Tags)
                        .Include(q => q.Author)
                        .Include(q => q.Institution)
                        .Include(q => q.Answers)
                        .Where(q => q.Institution.Id == institutionId && q.Status == QuestionStatus.Created);

                if (filter != null)
                {
                    queryable = filter.Apply(queryable);
                }

                return await queryable
                    .OrderByDescending(q => q.CreatedDate)
                    .Take(maxItems).ToListAsync();
            }
        }

        public async Task<IList<Question>> GetAllForStudentAsync(int userId, int maxItems = 10)
        {
            return await GetAllForStudentAsync(
                userId,
                null,
                maxItems);
        }

        public async Task<IList<Question>> GetAllForStudentAsync(
            int userId,
            IFilter<Question> filter,
            int maxItems = 10)
        {
            using (var context = new DatabaseContext())
            {
                var queryable =
                    context.Questions.Include(q => q.Tags)
                        .Include(q => q.Author)
                        .Include(q => q.Institution)
                        .Include(q => q.Answers)
                        .Where(q => q.Author.Id == userId && q.Status == QuestionStatus.Created);

                if (filter != null)
                {
                    queryable = filter.Apply(queryable);
                }

                return await queryable
                    .OrderByDescending(q => q.CreatedDate)
                    .Take(maxItems)
                    .ToListAsync();
            }
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.Questions.Include(q => q.Tags)
                            .Include(q => q.Author)
                            .Include(q => q.Institution)
                            .Include(q => q.Answers)
                            .SingleOrDefaultAsync(q => q.Id == id);
            }
        }

        public async Task<Question> UpdateAsync(Question item)
        {
            using (var context = new DatabaseContext())
            {
                var question =
                    await
                        context.Questions.Include(q => q.Tags)
                            .Include(q => q.Author)
                            .Include(q => q.Institution)
                            .Include(q => q.Answers)
                            .SingleOrDefaultAsync(i => i.Id == item.Id);

                if (question == null)
                {
                    return null;
                }

                question.Tags.Clear();

                foreach (var tag in item.Tags)
                {
                    var existingTag = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == tag.Id);

                    if (existingTag != null)
                    {
                        context.Entry(existingTag).CurrentValues.SetValues(tag);
                    }
                    else
                    {
                        context.TagRequests.Add(
                            new TagRequest
                            {
                                Institution = await context.Institutions.FindAsync(question.Institution.Id),
                                Text = tag.Text
                            });

                        existingTag = new Tag { Text = tag.Text };
                    }

                    question.Tags.Add(existingTag);
                }

                context.Entry(question).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return question;
            }
        }
    }
}