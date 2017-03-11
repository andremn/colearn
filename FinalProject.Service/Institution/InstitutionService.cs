using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service
{
    public class InstitutionService : IInstitutionService
    {
        private readonly IInstitutionDataAccess _institutionDataAccess;
        private readonly IInstitutionRequestDataAccess _institutionRequestDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public InstitutionService(IInstitutionDataAccess institutionDataAccess,
            IInstitutionRequestDataAccess institutionRequestDataAccess,
            IUnitOfWork unitOfWork)
        {
            if (institutionDataAccess == null)
            {
                throw new ArgumentNullException(nameof(institutionDataAccess));
            }

            if (institutionRequestDataAccess == null)
            {
                throw new ArgumentNullException(nameof(institutionRequestDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _institutionDataAccess = institutionDataAccess;
            _institutionRequestDataAccess = institutionRequestDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<InstitutionDataTransfer> CreateInstitutionAsync(
            InstitutionDataTransfer institutionDataTransfer)
        {
            if (institutionDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var institution = institutionDataTransfer.ToInstitution();

            institution = await _institutionDataAccess.CreateAsync(institution);

            await _unitOfWork.CommitAsync();
            return institution?.ToInstitutionDataTransfer();
        }

        public async Task<InstitutionRequestDataTransfer> CreateInstitutionRequestAsync(
            InstitutionRequestDataTransfer institutionRequestDataTransfer)
        {
            if (institutionRequestDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var institutionRequest = institutionRequestDataTransfer.ToInstitutionRequest();

            institutionRequest = await _institutionRequestDataAccess.CreateAsync(institutionRequest);

            await _unitOfWork.CommitAsync();
            return institutionRequest?.ToInstitutionRequestDataTransfer();
        }

        public async Task<InstitutionDataTransfer> GetInstitutionByIdAsync(int id)
        {
            var institution = await _institutionDataAccess.GetByIdAsync(id);

            return institution?.ToInstitutionDataTransfer();
        }

        public async Task<InstitutionDataTransfer> UpdateInstitutionAsync(
            InstitutionDataTransfer institutionDataTransfer)
        {
            if (institutionDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var institution = institutionDataTransfer.ToInstitution();

            institution = await _institutionDataAccess.UpdateAsync(institution);
            
            await _unitOfWork.CommitAsync();
            return institution?.ToInstitutionDataTransfer();
        }

        public async Task<IList<InstitutionDataTransfer>> GetAllInstitutionsAsync(
            IFilter<InstitutionDataTransfer> filter = null)
        {
            var institutionFilter = filter?.ConvertFilter<InstitutionDataTransfer, Institution>();

            var institutions = await _institutionDataAccess.GetAllAsync(institutionFilter);

            return institutions?
                .Select(i => i.ToInstitutionDataTransfer())
                .ToList();
        }

        public async Task<IList<InstitutionRequestDataTransfer>> GetAllInstitutionRequestsAsync(
            IFilter<InstitutionRequestDataTransfer> filter = null)
        {
            var convertedFilter = filter
                .ConvertFilter<InstitutionRequestDataTransfer, InstitutionRequest>();

            var institutionRequests = await _institutionRequestDataAccess.GetAllAsync(convertedFilter);

            return institutionRequests?
                .Select(i => i.ToInstitutionRequestDataTransfer())
                .ToList();
        }

        public async Task<IList<InstitutionRequestDataTransfer>> GetAllPendingInstitutionRequestsAsync()
        {
            var filter = new Filter<InstitutionRequest>(i => i.Status == InstitutionRequestStatus.Pending);
            var institutionRequests = await _institutionRequestDataAccess.GetAllAsync(filter);

            return institutionRequests?
                .Select(i => i.ToInstitutionRequestDataTransfer())
                .ToList();
        }

        public Task<long> GetPendingInstitutionRequestsCountAsync()
        {
            var filter = new Filter<InstitutionRequest>(
                i => i.Status == InstitutionRequestStatus.Pending);

            return _institutionRequestDataAccess.CountAsync(filter);
        }

        public async Task<InstitutionRequestDataTransfer> GetInstitutionRequestByIdAsync(int id)
        {
            var institutionRequest = await _institutionRequestDataAccess.GetByIdAsync(id);

            return institutionRequest?.ToInstitutionRequestDataTransfer();
        }

        public async Task<InstitutionRequestDataTransfer> UpdateInstitutionRequestAsync(
            InstitutionRequestDataTransfer institutionRequestDataTransfer)
        {
            if (institutionRequestDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var institutionRequest = institutionRequestDataTransfer.ToInstitutionRequest();

            institutionRequest = await _institutionRequestDataAccess.UpdateAsync(institutionRequest);

            await _unitOfWork.CommitAsync();
            return institutionRequest?.ToInstitutionRequestDataTransfer();
        }

        public async Task<IList<InstitutionRequestDataTransfer>> GetInstitutionsToModerateAsync()
        {
            var institutionRequests = await _institutionRequestDataAccess
                .GetAllAsync(
                    new Filter<InstitutionRequest>(
                        i => i.Status == InstitutionRequestStatus.Pending));

            return institutionRequests?
                .Select(i => i.ToInstitutionRequestDataTransfer())
                .ToList();
        }
    }
}