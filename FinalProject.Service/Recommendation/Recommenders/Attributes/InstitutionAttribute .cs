using System;
using System.Collections.Generic;
using System.Linq;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal class InstitutionAttribute : PreferenceAttribute
    {
        private readonly ICollection<InstitutionDataTransfer> _institutions;

        public InstitutionAttribute(ICollection<InstitutionDataTransfer> institutions)
        {
            if (institutions == null)
            {
                throw new ArgumentNullException(nameof(institutions));
            }

            _institutions = institutions;
        }

        public override float GetValueInStudentProfile(StudentDataTransfer student)
        {
            if (_institutions.Count == 0)
            {
                return 1f;
            }
            
            var hasInstitution = _institutions.Any(i => i.Id == student.Institution.Id);

            return hasInstitution ? 1f : 0f;
        }
    }
}
