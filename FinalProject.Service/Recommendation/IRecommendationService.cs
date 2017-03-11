using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IRecommendationService : IService
    {
        Task<IList<RecommendedStudentDataTransfer>> GetRecommendedInstructorsForStudentAsync(
            StudentDataTransfer target);
    }
}
