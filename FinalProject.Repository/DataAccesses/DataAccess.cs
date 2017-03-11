using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Factory;
using FinalProject.DataAccess.Filters;

namespace FinalProject.DataAccess
{
    public abstract class DataAccess<TKey, TItem> : IDataAccess<TKey, TItem>
        where TKey : struct
        where TItem : class
    {
        private readonly DbSet<TItem> _dbSet;

        private DatabaseContext _dataContext;

        protected IContextFactory Factory
        {
            get;
        }

        protected DatabaseContext Context => _dataContext ?? 
            (_dataContext = Factory.CreateContext<DatabaseContext>());

        protected DataAccess(IContextFactory factory)
        {
            Factory = factory;
            _dbSet = Context.Set<TItem>();
        }

        public virtual async Task<long> CountAsync(IFilter<TItem> filter = null)
        {
            var queryable = _dbSet.AsQueryable();

            queryable = filter?.Apply(queryable) ?? queryable;

            return await queryable
                .AsNoTracking()
                .LongCountAsync();
        }

        public virtual async Task<TItem> CreateAsync(TItem item)
        {
            return await Task.Run(() =>
            {
                var addedItem = _dbSet.Add(item);
                
                return addedItem;
            });
        }

        public virtual async Task<TItem> DeleteAsync(TItem item)
        {
            return await Task.Run(() => _dbSet.Remove(item));
        }

        public virtual async Task<TItem> DeleteByIdAsync(TKey id)
        {
            var item = await _dbSet.FindAsync(id);

            item = _dbSet.Remove(item);
            _dataContext.Entry(item).State = EntityState.Detached;

            return item;
        }

        public virtual async Task<IList<TItem>> GetAllAsync(IFilter<TItem> filter = null)
        {
            var queryable = _dbSet.AsQueryable();

            queryable = filter?.Apply(queryable) ?? queryable;

            return await queryable
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<TItem> GetByIdAsync(TKey id)
        {
            var item = await _dbSet.FindAsync(id);

            _dataContext.Entry(item).State = EntityState.Detached;
            return item;
        }

        public virtual async Task<TItem> UpdateAsync(TItem item)
        {
            return await Task.Run(() =>
            {
                _dataContext.Entry(item).State = EntityState.Modified;
                return item;
            });
        }
    }
}
