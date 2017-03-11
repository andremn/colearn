using System.Threading.Tasks;

namespace FinalProject.DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
