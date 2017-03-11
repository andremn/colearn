using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Factory;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerDataAccess _answerDataAccess;
        private readonly IAnswerRatingDataAccess _answerRatingDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public AnswerService(IAnswerDataAccess answerDataAccess, IAnswerRatingDataAccess answerRatingDataAccess, IUnitOfWork unitOfWork)
        {
            if (answerDataAccess == null)
            {
                throw new ArgumentNullException(nameof(answerDataAccess));
            }

            if (answerRatingDataAccess == null)
            {
                throw new ArgumentNullException(nameof(answerRatingDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _answerDataAccess = answerDataAccess;
            _answerRatingDataAccess = answerRatingDataAccess;
            _unitOfWork = unitOfWork;
        }

        public static async Task<AnswerDataTransfer> OnVideoAnswerProcessed(int answerId)
        {
            var context = new ContextFactory();
            var answerService = new AnswerService(new AnswerDataAccess(context), new AnswerRatingDataAccess(), new UnitOfWork(context));
            var questionService = new QuestionService(new QuestionDataAccess(), new UnitOfWork(context));
            var answer = await answerService.GetAnswerByIdAsync(answerId);
            var question = await questionService.GetQuestionByIdAsync(answer.Question.Id);

            question.Status = QuestionStatus.Created;
            await questionService.UpdateQuestionAsync(question);

            return answer;
        }

        public async Task<AnswerDataTransfer> CreateAnswerAsync(AnswerDataTransfer answerDataTransfer)
        {
            if (answerDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(answerDataTransfer));
            }

            var answer = answerDataTransfer.ToAnswer();

            answer = await _answerDataAccess.CreateAsync(answer);
            await _unitOfWork.CommitAsync();
            answer.Ratings = new List<AnswerRating>(0);
            return answer.ToAnswerDataTransfer();
        }

        public async Task<IList<AnswerDataTransfer>> GetAllUnseenAnswersForStudentAsync(
            int studentId, 
            DateTime? startTime = null)
        {
            startTime = startTime?.ToUniversalTime() ?? DateTime.MinValue;

            var filter = new Filter<Answer>(
                a => a.Question.Author.Id == studentId,
                a => a.CreatedDate >= startTime);

            var answers =  await _answerDataAccess.GetAllAsync(filter);

            return answers.Select(a => a.ToAnswerDataTransfer()).ToList();
        }

        public async Task<AnswerDataTransfer> GetAnswerByIdAsync(int id)
        {
            var answer = await _answerDataAccess.GetByIdAsync(id);

            return answer?.ToAnswerDataTransfer();
        }

        public async Task<IList<AnswerDataTransfer>> GetAllAnswersForQuestionAsync(int questionId)
        {
            var answers = await _answerDataAccess
                .GetAllAsync(new Filter<Answer>(a => a.Question.Id == questionId));

            return answers.Select(a => a.ToAnswerDataTransfer()).ToList();
        }

        public async Task<long> GetAnswersCount(IFilter<AnswerDataTransfer> filter)
        {
            var answerFilter = filter?.ConvertFilter<AnswerDataTransfer, Answer>();

            return await _answerDataAccess.CountAsync(answerFilter);
        }

        public async Task<AnswerRatingDataTransfer> RateAnswerAsync(AnswerRatingDataTransfer answerRatingDataTransfer)
        {
            if (answerRatingDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(answerRatingDataTransfer));
            }

            var answerRating = answerRatingDataTransfer.ToAnswerRating();

            answerRating = await _answerRatingDataAccess.CreateAsync(answerRating);
            await _unitOfWork.CommitAsync();
            return answerRating?.ToAnswerRatingDataTransfer();
        }

        public async Task<float> GetAvarageRatingByIdAsync(int id)
        {
            return await _answerDataAccess.GetAvarageRatingById(id);
        }

        public async Task<AnswerDataTransfer> UpdateAnswerAsync(AnswerDataTransfer answerDataTransfer)
        {
            var answer = answerDataTransfer.ToAnswer();

            answer = await _answerDataAccess.UpdateAsync(answer);
            await _unitOfWork.CommitAsync();
            return answer.ToAnswerDataTransfer();
        }
    }
}