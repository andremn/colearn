using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IAnswerService : IService
    {
        Task<AnswerDataTransfer> CreateAnswerAsync(AnswerDataTransfer answerDataTransfer);

        Task<IList<AnswerDataTransfer>> GetAllUnseenAnswersForStudentAsync(int studentId, DateTime? sinceDateTime = null);

        Task<AnswerDataTransfer> GetAnswerByIdAsync(int id);

        Task<IList<AnswerDataTransfer>> GetAllAnswersForQuestionAsync(int questionId);

        Task<long> GetAnswersCount(IFilter<AnswerDataTransfer> filter);

        Task<AnswerRatingDataTransfer> RateAnswerAsync(AnswerRatingDataTransfer answerRatingDataTransfer);

        Task<float> GetAvarageRatingByIdAsync(int id);

        Task<AnswerDataTransfer> UpdateAnswerAsync(AnswerDataTransfer answerDataTransfer);
    }
}