using FinalProject.Model;
using System;

namespace FinalProject.Service.Recommenders
{
    internal class GradeAttribute : PreferenceAttribute
    {
        private uint MIN_GRADE_ORDER = 1u;

        private readonly GradeDataTransfer _grade;

        public GradeAttribute(GradeDataTransfer grade)
        {
            _grade = grade;
        }

        public override float GetValueInStudentProfile(StudentDataTransfer student)
        {
            if (_grade == null)
            {
                return 1f;
            }

            var gradeOrder = student.Grade.Order;            
            var isOrderWithinPreference = gradeOrder <= _grade.Order;

            if (!isOrderWithinPreference)
            {
                return 0f;
            }

            var value = MIN_GRADE_ORDER / (float)gradeOrder;

            return value;
        }
    }
}
