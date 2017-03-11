using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service
{
    public class StudentService : IStudentService
    {
        private readonly IStudentDataAccess _studentDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IStudentDataAccess studentDataAccess, IUnitOfWork unitOfWork)
        {
            if (studentDataAccess == null)
            {
                throw new ArgumentNullException(nameof(studentDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _studentDataAccess = studentDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<StudentDataTransfer> CreateStudentAsync(StudentDataTransfer studentDataTransfer)
        {
            if (studentDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(studentDataTransfer));
            }

            var student = studentDataTransfer.ToStudent();

            student = await _studentDataAccess.CreateAsync(student);
            await _unitOfWork.CommitAsync();

            return student?.ToStudentDataTransfer();
        }

        public async Task<StudentDataTransfer> GetStudentByEmailAsync(string email)
        {
            var student = await _studentDataAccess.GetByEmailAsync(email);

            return student?.ToStudentDataTransfer();
        }

        public async Task<StudentDataTransfer> GetStudentByIdAsync(int id)
        {
            var student = await _studentDataAccess.GetByIdAsync(id);

            return student?.ToStudentDataTransfer();
        }

        public async Task<StudentDataTransfer> UpdateStudentAsync(StudentDataTransfer studentDataTransfer)
        {
            if (studentDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(studentDataTransfer));
            }

            var student = studentDataTransfer.ToStudent();

            student = await _studentDataAccess.UpdateAsync(student);
            await _unitOfWork.CommitAsync();

            return student?.ToStudentDataTransfer();
        }

        public async Task<StudentDataTransfer> DeleteStudentAsync(StudentDataTransfer studentDataTransfer)
        {
            if (studentDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(studentDataTransfer));
            }

            var student = studentDataTransfer.ToStudent();

            student = await _studentDataAccess.DeleteAsync(student);
            await _unitOfWork.CommitAsync();

            return student?.ToStudentDataTransfer();
        }

        public async Task<IList<StudentDataTransfer>> GetAllStudentsAsync(IFilter<StudentDataTransfer> filter = null)
        {
            var studentFilter = filter?.ConvertFilter<StudentDataTransfer, Student>();
            var students = await _studentDataAccess.GetAllAsync(studentFilter);
            var studentsDataTransfer = new List<StudentDataTransfer>(students.Count);

            foreach (var student in students)
            {
                var questionsTags = await _studentDataAccess.GetQuestionsTagsAsync(student.Id);
                var avgRating = await _studentDataAccess.GetAverageRatingAsync(student.Id);
                var studentDto = student.ToStudentDataTransfer();

                studentDto.AvgRating = avgRating;
                studentDto.QuestionTags = questionsTags
                    .Select(t => t.ToTagAcceptedDataTransfer())
                    .ToList();

                studentsDataTransfer.Add(studentDto);
            }

            return studentsDataTransfer;
        }
    }
}