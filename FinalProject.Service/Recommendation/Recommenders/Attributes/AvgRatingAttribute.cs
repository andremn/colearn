using System;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal class AvgRatingAttribute : PreferenceAttribute
    {
        private const float MaxRatingValue = 5f;

        private readonly float _avgRating;

        public AvgRatingAttribute(float avgRating)
        {
            if (!(avgRating > 0))
            {
                throw new ArgumentException("Value should be greater than zero.");
            }

            _avgRating = avgRating;
        }

        public override float GetValueInStudentProfile(StudentDataTransfer student)
        {
            var rating = student.AvgRating;

            if (rating < 1f)
            {
                return 0f;
            }

            var isRatingWithinPreference = rating >= _avgRating;

            if (!isRatingWithinPreference)
            {
                return 0f;
            }

            var value = rating / MaxRatingValue;

            return value * 2;
        }
    }
}
