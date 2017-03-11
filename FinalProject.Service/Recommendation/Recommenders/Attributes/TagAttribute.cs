using System;
using System.Collections.Generic;
using System.Linq;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal class TagAttribute : PreferenceAttribute
    {
        private readonly ICollection<TagAcceptedDataTransfer> _tags;

        public TagAttribute(ICollection<TagAcceptedDataTransfer> tags)
        {
            _tags = tags;
        }

        public override float GetValueInStudentProfile(StudentDataTransfer student)
        {
            if (student.InstructorTags == null || _tags.Count == 0)
            {
                return 0f;
            }

            var value = student.InstructorTags
                .Aggregate(0f, (current, tag) =>
                    current + _tags.Count(t => t.Text.Equals(tag.Text, StringComparison.InvariantCultureIgnoreCase)));

            value = value > 0f ? value / _tags.Count : 0f;

            return value * 2;
        }
    }
}
