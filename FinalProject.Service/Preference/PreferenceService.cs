using System;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service
{
    internal class PreferenceService : IPreferenceService
    {
        private readonly IPreferenceDataAccess _preferenceDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public PreferenceService(IPreferenceDataAccess preferenceDataAccess, IUnitOfWork unitOfWork)
        {
            if (preferenceDataAccess == null)
            {
                throw new ArgumentNullException(nameof(preferenceDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _preferenceDataAccess = preferenceDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<PreferenceDataTransfer> CreatePreferenceAsync(PreferenceDataTransfer preferenceDataTransfer)
        {
            var preference = preferenceDataTransfer.ToPreference();

            preference = await _preferenceDataAccess.CreateAsync(preference);
            await _unitOfWork.CommitAsync();

            return preference?.ToPreferenceDataTransfer();
        }

        public async Task<PreferenceDataTransfer> GetPreferenceByStudentId(int id)
        {
            var preference = await _preferenceDataAccess.GetByStudentIdOrDefaultAsync(id);

            return preference?.ToPreferenceDataTransfer();
        }

        public async Task<PreferenceDataTransfer> ResetPrefereceForStudentAsync(int studentId)
        {
            var preference = await GetPreferenceByStudentId(studentId);
            var defaultPreference = await GetDefaultPreferenceAsync();

            defaultPreference.Student = preference.Student;
            defaultPreference.Id = preference.Id;

            return await UpdatePreferenceAsync(defaultPreference);
        }

        public async Task<PreferenceDataTransfer> UpdatePreferenceAsync(PreferenceDataTransfer preferenceDataTransfer)
        {
            var preference = preferenceDataTransfer.ToPreference();
            var existingPreference = await _preferenceDataAccess.GetByIdAsync(preference.Id);

            if (existingPreference == null)
            {
                return await CreatePreferenceAsync(preferenceDataTransfer);
            }

            preference.Id = existingPreference.Id;
            preference.IsDefault = existingPreference.IsDefault;
            preference = await _preferenceDataAccess.UpdateAsync(preference);

            await _unitOfWork.CommitAsync();

            return preference.ToPreferenceDataTransfer();
        }

        public async Task<PreferenceDataTransfer> GetDefaultPreferenceAsync()
        {
            var defaultPreference = await _preferenceDataAccess.GetDefaultAsync();

            return defaultPreference.ToPreferenceDataTransfer();
        }
    }
}