using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IAnswerRatingDataAccess : IDataAccess<int, AnswerRating>
    {
    }

    public class AnswerRatingDataAccess : IAnswerRatingDataAccess
    {
        public async Task<long> CountAsync(IFilter<AnswerRating> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.AnswerRatings.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<AnswerRating> CreateAsync(AnswerRating item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Author == null || item.Answer == null)
                {
                    throw new ArgumentException(
                        $@"The fields '{nameof(item.Author)}' and 
                                                '{nameof
                            (item.Answer)}' are required");
                }

                item.Author = await context.Students.FindAsync(item.Author.Id);

                if (item.Answer is TextAnswer)
                {
                    item.Answer = await context.Answers.FindAsync(item.Answer.Id);
                }
                else
                {
                    item.Answer = await context.Answers.FindAsync(item.Answer.Id);
                }

                var rating = context.AnswerRatings.Add(item);

                await context.SaveChangesAsync();
                return rating;
            }
        }

        public async Task<AnswerRating> DeleteAsync(AnswerRating item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<AnswerRating> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var rating =
                    await
                        context.AnswerRatings.Include(a => a.Author)
                            .Include(a => a.Answer)
                            .SingleOrDefaultAsync(institution => institution.Id == id);

                rating = context.AnswerRatings.Remove(rating);
                await context.SaveChangesAsync();
                return rating;
            }
        }

        public async Task<IList<AnswerRating>> GetAllAsync(IFilter<AnswerRating> filter = null)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.AnswerRatings
                    .Include(a => a.Author)
                    .Include(a => a.Answer.Author);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<AnswerRating> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.AnswerRatings.Include(a => a.Author)
                            .Include(a => a.Answer)
                            .SingleOrDefaultAsync(q => q.Id == id);
            }
        }

        public async Task<AnswerRating> UpdateAsync(AnswerRating item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Author == null || item.Answer == null)
                {
                    throw new ArgumentException(
                        $@"The fields '{nameof(item.Author)}' and 
                                                '{nameof
                            (item.Answer)}' are required");
                }

                var rating =
                    await
                        context.AnswerRatings.Include(a => a.Author)
                            .Include(a => a.Answer)
                            .SingleOrDefaultAsync(a => a.Id == item.Id);

                context.Entry(rating).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return rating;
            }
        }
    }
}