using FinalProject.DataAccess.Factory;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IGradeDataAccess : IDataAccess<int, Grade>
    {
        
    }

    internal class GradeDataAccess : DataAccess<int, Grade>, IGradeDataAccess
    {
        public GradeDataAccess(IContextFactory factory) 
            : base(factory)
        {
        }
    }
}
