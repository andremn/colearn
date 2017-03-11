using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Factory;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IAnswerDataAccess : IDataAccess<int, Answer>
    {
        Task<float> GetAvarageRatingById(int id);
    }

    public class AnswerDataAccess : DataAccess<int, Answer>, IAnswerDataAccess
    {
        public AnswerDataAccess(IContextFactory factory) 
            : base(factory)
        {
        }

        public override async Task<Answer> CreateAsync(Answer item)
        {
            if (item.Author == null)
            {
                throw new ArgumentException($@"The field '{nameof(item.Author)}' is required");
            }

            item.Author = await Context.Students.FindAsync(item.Author.Id);

            if (item.Question != null)
            {
                item.Question = await Context.Questions.FindAsync(item.Question.Id);
                Context.Questions.Attach(item.Question);
            }

            Context.Students.Attach(item.Author);

            var answer = Context.Answers.Add(item);

            return answer;
        }

        public override async Task<Answer> DeleteByIdAsync(int id)
        {
            var answer = await Context.Answers
                .Include(q => q.Author)
                .Include(a => a.Question)
                .Include(a => a.Ratings)
                .SingleOrDefaultAsync(institution => institution.Id == id);
            
            return await DeleteAsync(answer);
        }

        public override async Task<IList<Answer>> GetAllAsync(IFilter<Answer> filter = null)
        {
            var queryable = Context.Answers.Include(q => q.Author)
                .Include(a => a.Question)
                .Include(a => a.Ratings.Select(r => r.Author));

            queryable = filter?.Apply(queryable) ?? queryable;

            return await queryable
                .OrderByDescending(q => q.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<float> GetAvarageRatingById(int id)
        {
            var answer = await Context.Answers
                .Include(a => a.Ratings)
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Id == id);

            var sum = answer.Ratings.Sum(r => r.Value);

            return sum / answer.Ratings.Count;
        }

        public override async Task<Answer> GetByIdAsync(int id)
        {
            return await Context.Answers
                    .Include(q => q.Author)
                    .Include(a => a.Question)
                    .Include(a => a.Ratings.Select(r => r.Author))
                    .AsNoTracking()
                    .SingleOrDefaultAsync(q => q.Id == id);
        }

        public override async Task<Answer> UpdateAsync(Answer item)
        {
            if (item.Author == null)
            {
                throw new ArgumentException($@"The field '{nameof(item.Author)}' is required");
            }

            var answer = await Context.Answers
                .SingleOrDefaultAsync(a => a.Id == item.Id);

            if (item.Question != null)
            {
                Context.Questions.Attach(item.Question);
                answer.Question = item.Question;
            }

            Context.Entry(answer).CurrentValues.SetValues(item);
            return answer;
        }
    }
}