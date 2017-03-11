using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;

namespace FinalProject.DataAccess
{
    public interface IDataAccess
    {
    }

    public interface IDataAccess<in TId, TItem> : IDataAccess
        where TId : struct
        where TItem : class
    {
        Task<long> CountAsync(IFilter<TItem> filter = null);

        Task<TItem> CreateAsync(TItem item);

        Task<TItem> DeleteAsync(TItem item);

        Task<TItem> DeleteByIdAsync(TId id);

        Task<IList<TItem>> GetAllAsync(IFilter<TItem> filter = null);

        Task<TItem> GetByIdAsync(TId id);

        Task<TItem> UpdateAsync(TItem item);
    }
}