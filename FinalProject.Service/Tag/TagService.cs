using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;
using FinalProject.Service.Notification;

namespace FinalProject.Service
{
    internal class TagService : ITagService
    {
        private readonly ITagDataAccess _tagDataAccess;
        private readonly ITagRequestDataAccess _tagRequestDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public TagService(
            ITagDataAccess tagDataAccess,
            ITagRequestDataAccess tagRequestDataAccess,
            IUnitOfWork unitOfWork)
        {
            if (tagDataAccess == null)
            {
                throw new ArgumentNullException(nameof(tagDataAccess));
            }

            if (tagRequestDataAccess == null)
            {
                throw new ArgumentNullException(nameof(tagRequestDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _tagDataAccess = tagDataAccess;
            _tagRequestDataAccess = tagRequestDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<TagAcceptedDataTransfer> CreateTagAsync(TagAcceptedDataTransfer tagDataTransfer)
        {
            if (tagDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(tagDataTransfer));
            }

            var tag = tagDataTransfer.ToTag();

            tag = await _tagDataAccess.CreateAsync(tag);
            await _unitOfWork.CommitAsync();

            return tag?.ToTagAcceptedDataTransfer();
        }

        public async Task<TagRequestDataTransfer> CreateTagRequestAsync(TagRequestDataTransfer tagRequestDataTransfer)
        {
            if (tagRequestDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(tagRequestDataTransfer));
            }

            var tagRequest = tagRequestDataTransfer.ToTagRequest();

            tagRequest = await _tagRequestDataAccess.CreateAsync(tagRequest);
            await _unitOfWork.CommitAsync();

            tagRequestDataTransfer = tagRequest?.ToTagRequestDataTransfer();
            NotifyTagRequestChangedAsync(tagRequestDataTransfer);

            return tagRequestDataTransfer;
        }

        public async Task<TagAcceptedDataTransfer> GetTagByIdAsync(int id)
        {
            var tag = await _tagDataAccess.GetByIdAsync(id);

            return tag?.ToTagAcceptedDataTransfer();
        }

        public async Task<IList<TagAcceptedDataTransfer>> GetTagsByInstitutionIdAsync(int id)
        {
            var tag = await _tagDataAccess.GetAllForInstitutionAsync(id);

            return tag?.Select(t => t.ToTagAcceptedDataTransfer())
                .ToList();
        }

        public async Task<TagAcceptedDataTransfer> UpdateTagAsync(TagAcceptedDataTransfer tagDataTransfer)
        {
            var tag = tagDataTransfer.ToTag();

            tag = await _tagDataAccess.UpdateAsync(tag);
            await _unitOfWork.CommitAsync();

            return tag?.ToTagAcceptedDataTransfer();
        }

        public async Task<TagRequestDataTransfer> UpdateTagRequestAsync(TagRequestDataTransfer tagRequestDataTransfer)
        {
            var tagRequest = tagRequestDataTransfer.ToTagRequest();

            tagRequest = await _tagRequestDataAccess.UpdateAsync(tagRequest);
            await _unitOfWork.CommitAsync();

            tagRequestDataTransfer = tagRequest?.ToTagRequestDataTransfer();
            NotifyTagRequestChangedAsync(tagRequestDataTransfer);

            return tagRequestDataTransfer;
        }

        public async Task<TagAcceptedDataTransfer> GetTagByNameForInstitutionAsync(string name, int institutionId)
        {
            var tag = await _tagDataAccess.GetTagByNameForInstitutionAsync(
                name,
                institutionId);

            return tag?.ToTagAcceptedDataTransfer();
        }

        public async Task<TagAcceptedDataTransfer> DeleteTagAsync(TagAcceptedDataTransfer tagDataTransfer)
        {
            var tag = tagDataTransfer.ToTag();

            tag = await _tagDataAccess.DeleteAsync(tag);
            await _unitOfWork.CommitAsync();

            return tag?.ToTagAcceptedDataTransfer();
        }

        public async Task<IList<TagRequestDataTransfer>> GetAllTagsPendingForInstitutionAsync(int institutionId)
        {
            var tagRequests = await _tagRequestDataAccess.GetAllPendingAsync(institutionId);

            return tagRequests?.Select(t => t.ToTagRequestDataTransfer())
                .ToList();
        }

        public Task<long> GetTagsPendingCountForInstitutionAsync(int institutionId)
        {
            var filter = new Filter<TagRequest>(
                r => r.Institution.Id == institutionId &&
                     r.Status == TagRequestStatus.Pending);

            return _tagRequestDataAccess.CountAsync(filter);
        }

        public async Task<IList<TagAcceptedDataTransfer>> GetAllTagsAsync(IFilter<TagAcceptedDataTransfer> filter)
        {
            var tagFilter = filter?.ConvertFilter<TagAcceptedDataTransfer, Tag>();
            var tags = await _tagDataAccess.GetAllAsync(tagFilter);

            return tags?.Select(t => t.ToTagAcceptedDataTransfer())
                .ToList();
        }

        public async Task<IList<TagAcceptedDataTransfer>> GetAllTagsForInstitutionAsync(int id)
        {
            var tags = await _tagDataAccess.GetAllForInstitutionAsync(id);

            return tags?.Select(t => t.ToTagAcceptedDataTransfer())
                .ToList();
        }

        public async Task<TagRequestDataTransfer> GetTagRequestByIdAsync(int id)
        {
            var tagRequest = await _tagRequestDataAccess.GetByIdAsync(id);

            return tagRequest?.ToTagRequestDataTransfer();
        }

        private void NotifyTagRequestChangedAsync(TagRequestDataTransfer tagRequest)
        {
            Task.Run(async () =>
            {
                var tagRequestsCount = await _tagRequestDataAccess.CountPendingAsync();
                var notificationData = new TagRequestNotificationData(tagRequest, tagRequestsCount);

                NotificationListener.Instance.NotifyCategoryListenersAsync(
                    NotificationCategories.NewTagsRequests,
                    new NotificationEvent(notificationData));
            });
        }
    }
}