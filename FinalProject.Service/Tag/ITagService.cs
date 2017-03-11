using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface ITagService : IService
    {
        Task<TagAcceptedDataTransfer> CreateTagAsync(TagAcceptedDataTransfer tagDataTransfer);

        Task<TagRequestDataTransfer> CreateTagRequestAsync(TagRequestDataTransfer tagDataTransfer);

        Task<TagAcceptedDataTransfer> GetTagByIdAsync(int tagId);

        Task<IList<TagAcceptedDataTransfer>> GetTagsByInstitutionIdAsync(int institutionId);

        Task<TagAcceptedDataTransfer> UpdateTagAsync(TagAcceptedDataTransfer tagDataTransfer);

        Task<TagRequestDataTransfer> UpdateTagRequestAsync(TagRequestDataTransfer tagDataTransfer);

        Task<TagAcceptedDataTransfer> GetTagByNameForInstitutionAsync(string tag, int institutionId);

        Task<TagAcceptedDataTransfer> DeleteTagAsync(TagAcceptedDataTransfer tagDataTransfer);

        Task<IList<TagRequestDataTransfer>> GetAllTagsPendingForInstitutionAsync(int institutionId);

        Task<long> GetTagsPendingCountForInstitutionAsync(int institutionId);

        Task<IList<TagAcceptedDataTransfer>> GetAllTagsAsync(IFilter<TagAcceptedDataTransfer> filter);

        Task<IList<TagAcceptedDataTransfer>> GetAllTagsForInstitutionAsync(int id);

        Task<TagRequestDataTransfer> GetTagRequestByIdAsync(int id);
    }
}