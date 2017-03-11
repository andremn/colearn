using System;
using System.Collections.Generic;
using System.Linq;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal class StudentRecommender : IStudentRecommender
    {
        private const float MinSimilarityValue = 0.0001f;

        private readonly ICollection<PreferenceDataTransfer> _preferences;

        public StudentRecommender(ICollection<PreferenceDataTransfer> preferences)
        {
            if (preferences == null)
            {
                throw new ArgumentNullException(nameof(preferences));
            }

            _preferences = preferences;
        }

        public IList<RecommendedStudentDataTransfer> GetRecommenderStudentsForStudentPreferences(
            PreferenceDataTransfer target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (target.Tags == null || target.Tags.Count == 0)
            {
                return new List<RecommendedStudentDataTransfer>(0);
            }

            var preferences = _preferences;

            // If the student we're recommending instructors to are in the list, remove it
            if (_preferences.Any(s => s.Student.Id == target.Student.Id))
            {
                preferences = _preferences
                   .Where(s => s.Student.Id != target.Id)
                   .ToArray();
            }

            var recommendedStudents = new List<RecommendedStudentDataTransfer>();

            foreach (var preference in preferences)
            {
                // No tags? Skip it...
                if (preference?.Student.InstructorTags == null || 
                    preference.Student.InstructorTags.Count == 0)
                {
                    continue;
                }

                // Calculate the similarity between student 'target' and the current student
                // The similarity value represents how the student 'target' is similar to the 
                // current student
                var similarity = CalculateSimilarity(target, preference.Student);

                if (!(similarity > 0))
                {
                    continue;
                }

                var student = preference.Student.ToRecommendedStudentDataTransfer();

                student.QuestionTags = preference.Tags;
                student.Similarity = similarity;
                recommendedStudents.Add(student);
            }

            recommendedStudents.Sort(new RecommendedStudentsComparer());

            return recommendedStudents;
        }

        private float CalculateSimilarity(
            PreferenceDataTransfer preferences,
            StudentDataTransfer student)
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            var modelBuilder = new PreferenceModelBuilder();
            var attributes = modelBuilder.GetPreferenceAttributes(preferences);
            var tagsAttribute = attributes?.SingleOrDefault(a => a is TagAttribute);

            if (tagsAttribute == null)
            {
                return 0f;
            }

            var tagsAttributeValue = tagsAttribute.GetValueInStudentProfile(student);

            if (Math.Abs(tagsAttributeValue) < MinSimilarityValue)
            {
                return MinSimilarityValue;
            }

            var preferencesValuesSum = attributes
                .Sum(attribute => attribute.GetValueInStudentProfile(student));

            return preferencesValuesSum / (attributes.Count + 2);
        }

        private class RecommendedStudentsComparer : IComparer<RecommendedStudentDataTransfer>
        {
            public int Compare(RecommendedStudentDataTransfer x, RecommendedStudentDataTransfer y)
            {
                var similarityCompareValue = y.Similarity.CompareTo(x.Similarity);

                return similarityCompareValue != 0 
                    ? similarityCompareValue 
                    : y.AvgRating.CompareTo(x.AvgRating);
            }
        }
    }
}
