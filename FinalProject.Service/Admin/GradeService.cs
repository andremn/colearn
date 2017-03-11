using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service.Admin
{
    public class GradeService : IGradeService
    {
        private readonly IGradeDataAccess _gradeDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public GradeService(IGradeDataAccess gradeDataAccess, IUnitOfWork unitOfWork)
        {
            if (gradeDataAccess == null)
            {
                throw new ArgumentNullException(nameof(gradeDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _gradeDataAccess = gradeDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<GradeDataTransfer> GetGradeByIdAsync(int id)
        {
            var grade = await _gradeDataAccess.GetByIdAsync(id);

            return grade?.ToGradeDataTransfer();
        }

        public async Task<GradeDataTransfer> UpdateGradeAsync(GradeDataTransfer gradeDataTransfer)
        {
            if (gradeDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(gradeDataTransfer));
            }

            var grade = gradeDataTransfer.ToGrade();

            grade = await _gradeDataAccess.UpdateAsync(grade);
            await _unitOfWork.CommitAsync();

            return grade.ToGradeDataTransfer();
        }

        public async Task<GradeDataTransfer> CreateGradeAsync(GradeDataTransfer gradeDataTransfer)
        {
            if (gradeDataTransfer == null)
            {
                throw new ArgumentNullException(nameof(gradeDataTransfer));
            }

            var grade = gradeDataTransfer.ToGrade();

            grade = await _gradeDataAccess.CreateAsync(grade);
            await _unitOfWork.CommitAsync();

            return grade.ToGradeDataTransfer();
        }

        public async Task<IList<GradeDataTransfer>> GetAllGradesAsync(IFilter<GradeDataTransfer> filter = null)
        {
            var dataAccessFilter = filter?.ConvertFilter<GradeDataTransfer, Grade>();

            var grades = await _gradeDataAccess.GetAllAsync(dataAccessFilter);

            return grades?.Select(g => g.ToGradeDataTransfer()).ToList()
                   ?? new List<GradeDataTransfer>(0);
        }
    }
}
