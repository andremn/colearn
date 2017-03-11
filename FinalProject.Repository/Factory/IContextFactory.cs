using System;
using System.Data.Entity;

namespace FinalProject.DataAccess.Factory
{
    public interface IContextFactory : IDisposable
    {
        TContext CreateContext<TContext>() 
            where TContext : DbContext, new();
    }
}
