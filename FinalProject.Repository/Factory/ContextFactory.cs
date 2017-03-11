using System.Data.Entity;
using FinalProject.Shared.ObjectModel;

namespace FinalProject.DataAccess.Factory
{
    public class ContextFactory : Disposable, IContextFactory
    {
        private DbContext _currentContext;

        public TContext CreateContext<TContext>()
            where TContext : DbContext, new()
        {
            return (TContext)(_currentContext ?? 
                (_currentContext = new TContext()));
        }

        protected override void DisposeCore()
        {
            _currentContext?.Dispose();
        }
    }
}
