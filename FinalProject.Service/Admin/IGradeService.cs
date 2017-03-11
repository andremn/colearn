using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service.Admin
{
    public interface IGradeService : IService
    {
        Task<GradeDataTransfer> CreateGradeAsync(GradeDataTransfer gradeDataTransfer);

        Task<IList<GradeDataTransfer>> GetAllGradesAsync(IFilter<GradeDataTransfer> filter = null);

        Task<GradeDataTransfer> GetGradeByIdAsync(int id);

        Task<GradeDataTransfer> UpdateGradeAsync(GradeDataTransfer gradeDataTransfer);
    }
}