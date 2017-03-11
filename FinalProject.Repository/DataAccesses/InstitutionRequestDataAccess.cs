using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IInstitutionRequestDataAccess : IDataAccess<int, InstitutionRequest>
    {
        Task<int> CountPendingAsync();

        Task<InstitutionRequest> GetByCodeAsync(int code);

        Task<InstitutionRequest> GetByOwnerEmailAsync(string email);

        Task<IList<InstitutionRequest>> GetPendingAsync();
    }

    internal class InstitutionRequestDataAccess : IInstitutionRequestDataAccess
    {
        public async Task<long> CountAsync(IFilter<InstitutionRequest> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.InstitutionRequests.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<int> CountPendingAsync()
        {
            using (var context = new DatabaseContext())
            {
                return await context.InstitutionRequests.CountAsync(i => i.Status == InstitutionRequestStatus.Pending);
            }
        }

        public async Task<InstitutionRequest> CreateAsync(InstitutionRequest item)
        {
            using (var context = new DatabaseContext())
            {
                var institutionRequest = context.InstitutionRequests.Add(item);

                await context.SaveChangesAsync();
                return institutionRequest;
            }
        }

        public async Task<InstitutionRequest> DeleteAsync(InstitutionRequest item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<InstitutionRequest> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var institutionRequest = context.InstitutionRequests.Find(id);

                institutionRequest = context.InstitutionRequests.Remove(institutionRequest);
                await context.SaveChangesAsync();
                return institutionRequest;
            }
        }

        public async Task<IList<InstitutionRequest>> GetAllAsync(IFilter<InstitutionRequest> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.InstitutionRequests.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<InstitutionRequest> GetByCodeAsync(int code)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.InstitutionRequests.Where(institution => institution.InstitutionCode == code)
                            .SingleOrDefaultAsync();
            }
        }

        public async Task<InstitutionRequest> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.InstitutionRequests.SingleOrDefaultAsync(
                            institutionManager => institutionManager.Id == id);
            }
        }

        public async Task<InstitutionRequest> GetByOwnerEmailAsync(string email)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.InstitutionRequests.Where(institution => institution.OwnerEmail == email)
                            .SingleOrDefaultAsync();
            }
        }

        public async Task<IList<InstitutionRequest>> GetPendingAsync()
        {
            return await GetAllAsync(
                new Filter<InstitutionRequest>(
                    institution => institution.Status == InstitutionRequestStatus.Pending));
        }

        public async Task<InstitutionRequest> UpdateAsync(InstitutionRequest item)
        {
            using (var context = new DatabaseContext())
            {
                var institutionRequest = await context.InstitutionRequests.FindAsync(item.Id);

                context.Entry(institutionRequest).CurrentValues.SetValues(item);

                await context.SaveChangesAsync();
                return institutionRequest;
            }
        }
    }
}