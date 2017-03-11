using System.Threading.Tasks;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IPreferenceService : IService
    {
        Task<PreferenceDataTransfer> CreatePreferenceAsync(PreferenceDataTransfer preferenceDataTransfer);

        Task<PreferenceDataTransfer> GetPreferenceByStudentId(int id);

        Task<PreferenceDataTransfer> ResetPrefereceForStudentAsync(int studentId);

        Task<PreferenceDataTransfer> UpdatePreferenceAsync(PreferenceDataTransfer preferenceDataTransfer);

        Task<PreferenceDataTransfer> GetDefaultPreferenceAsync();
    }
}