using System.Collections.Generic;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface IStudentService : IService
    {
        Task<StudentDataTransfer> CreateStudentAsync(StudentDataTransfer studentDataTransfer);

        Task<StudentDataTransfer> GetStudentByEmailAsync(string email);

        Task<StudentDataTransfer> GetStudentByIdAsync(int id);

        Task<StudentDataTransfer> UpdateStudentAsync(StudentDataTransfer studentDataTransfer);

        Task<StudentDataTransfer> DeleteStudentAsync(StudentDataTransfer studentDataTransfer);

        Task<IList<StudentDataTransfer>> GetAllStudentsAsync(IFilter<StudentDataTransfer> filter = null);
    }
}