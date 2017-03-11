using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IQuestionService : IService
    {
        Task<QuestionDataTransfer> CreateQuestionAsync(QuestionDataTransfer questionDataTransfer);

        Task<QuestionDataTransfer> GetQuestionByIdAsync(int id);

        Task<IList<QuestionDataTransfer>> GetAllQuestionsAsync();

        Task<QuestionDataTransfer> UpdateQuestionAsync(QuestionDataTransfer questionDataTransfer);

        Task<IList<QuestionDataTransfer>> GetAllQuestionsForInstitutionAsync(
            int institutionId,
            IFilter<QuestionDataTransfer> filter, int maxItems);

        Task<IList<QuestionDataTransfer>> GetAllQuestionsForStudentAsync(
            int studentId,
            IFilter<QuestionDataTransfer> filter,
            int maxItems);

        Task<IList<QuestionDataTransfer>> GetAllQuestionsRelatedForStudentAsync(
            StudentDataTransfer student,
            IFilter<QuestionDataTransfer> filter,
            int maxItems);

        Task<IList<QuestionDataTransfer>> GetAllContributedQuestionsForStudentAsync(
            int studentId,
            IFilter<QuestionDataTransfer> filter,
            int maxItems);
    }
}