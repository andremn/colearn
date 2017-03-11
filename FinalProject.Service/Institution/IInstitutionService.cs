using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IInstitutionService : IService
    {
        Task<InstitutionDataTransfer> CreateInstitutionAsync(InstitutionDataTransfer institutionDataTransfer);

        Task<InstitutionRequestDataTransfer> CreateInstitutionRequestAsync(
            InstitutionRequestDataTransfer institutionRequestDataTransfer);

        Task<InstitutionDataTransfer> GetInstitutionByIdAsync(int id);

        Task<InstitutionDataTransfer> UpdateInstitutionAsync(InstitutionDataTransfer institutionDataTransfer);

        Task<IList<InstitutionDataTransfer>> GetAllInstitutionsAsync(IFilter<InstitutionDataTransfer> filter = null);

        Task<IList<InstitutionRequestDataTransfer>> GetAllInstitutionRequestsAsync(
            IFilter<InstitutionRequestDataTransfer> filter = null);

        Task<IList<InstitutionRequestDataTransfer>> GetAllPendingInstitutionRequestsAsync();

        Task<long> GetPendingInstitutionRequestsCountAsync();

        Task<InstitutionRequestDataTransfer> GetInstitutionRequestByIdAsync(int id);

        Task<InstitutionRequestDataTransfer> UpdateInstitutionRequestAsync(
            InstitutionRequestDataTransfer institutionRequestDataTransfer);

        Task<IList<InstitutionRequestDataTransfer>> GetInstitutionsToModerateAsync();
    }
}