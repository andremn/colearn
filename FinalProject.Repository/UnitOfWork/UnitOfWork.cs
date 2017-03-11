using System.Threading.Tasks;
using FinalProject.DataAccess.Factory;

namespace FinalProject.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IContextFactory _contextFactory;

        private DatabaseContext _context;

        public UnitOfWork(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        internal DatabaseContext Context => _context ?? 
            (_context = _contextFactory.CreateContext<DatabaseContext>());

        public async Task CommitAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
