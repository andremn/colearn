using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface ITagRequestDataAccess : IDataAccess<int, TagRequest>
    {
        Task<int> CountPendingAsync();

        Task<IList<TagRequest>> GetAllPendingAsync(int institutionId);

        Task<TagRequest> GetByNameAsync(string text);
    }

    internal class TagRequestDataAccess : ITagRequestDataAccess
    {
        public async Task<long> CountAsync(IFilter<TagRequest> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.TagRequests.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<int> CountPendingAsync()
        {
            using (var context = new DatabaseContext())
            {
                return await context.TagRequests.CountAsync(t => t.Status == TagRequestStatus.Pending);
            }
        }

        public async Task<TagRequest> CreateAsync(TagRequest item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Institution == null)
                {
                    throw new ArgumentException(
                        $"The field {nameof(item.Institution)} is required.");
                }

                context.Institutions.Attach(item.Institution);

                if (item.Question != null)
                {
                    context.Questions.Attach(item.Question);
                }

                if (item.Author != null)
                {
                    context.Students.Attach(item.Author);
                }

                var tagRequest = context.TagRequests.Add(item);

                await context.SaveChangesAsync();

                return tagRequest;
            }
        }

        public async Task<TagRequest> DeleteAsync(TagRequest item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<TagRequest> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var tagRequest =
                    await
                        context.TagRequests.Include(t => t.Institution)
                            .Include(t => t.Question)
                            .Include(t => t.Author)
                            .SingleOrDefaultAsync(t => t.Id == id);

                tagRequest = context.TagRequests.Remove(tagRequest);
                await context.SaveChangesAsync();
                return tagRequest;
            }
        }

        public async Task<IList<TagRequest>> GetAllAsync(IFilter<TagRequest> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.TagRequests
                    .Include(t => t.Institution)
                    .Include(t => t.Question)
                    .Include(t => t.Author);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<IList<TagRequest>> GetAllPendingAsync(int institutionId)
        {
            return await GetAllAsync(
                new Filter<TagRequest>(
                    request =>
                        request.Status == TagRequestStatus.Pending &&
                        request.Institution.Id == institutionId));
        }

        public async Task<TagRequest> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.TagRequests.Include(t => t.Institution)
                            .Include(t => t.Question)
                            .Include(t => t.Author)
                            .SingleOrDefaultAsync(t => t.Id == id);
            }
        }

        public async Task<TagRequest> GetByNameAsync(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.TagRequests.Where(tag => tag.Text == text)
                            .Include(t => t.Institution)
                            .Include(t => t.Question)
                            .Include(t => t.Author)
                            .SingleOrDefaultAsync();
            }
        }

        public async Task<TagRequest> UpdateAsync(TagRequest item)
        {
            using (var context = new DatabaseContext())
            {
                var tagReqeuest =
                    await
                        context.TagRequests.Include(t => t.Institution)
                            .Include(t => t.Question)
                            .Include(t => t.Author)
                            .SingleOrDefaultAsync(t => t.Id == item.Id);

                if (tagReqeuest == null)
                {
                    return null;
                }

                if (item.Institution != null)
                {
                    var institution =
                        await
                            context.Institutions.Include(i => i.Tags)
                                .SingleOrDefaultAsync(i => i.Id == item.Institution.Id);

                    if (institution != null)
                    {
                        item.Institution = institution;
                    }
                }

                context.Entry(tagReqeuest).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return tagReqeuest;
            }
        }
    }
}