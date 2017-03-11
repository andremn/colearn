using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;
using FinalProject.Service.Recommenders;

namespace FinalProject.Service
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IStudentDataAccess _studentDataAccess;
        private readonly IPreferenceDataAccess _preferenceDataAccess;
        private readonly IAnswerRatingDataAccess _answerRatingDataAccess;
        private readonly IGradeDataAccess _gradeDataAccess;

        public RecommendationService(
            IStudentDataAccess studentDataAccess, 
            IPreferenceDataAccess preferenceDataAccess,
            IAnswerRatingDataAccess answerRatingDataAccess,
            IGradeDataAccess gradeDataAccess)
        {
            if (studentDataAccess == null)
            {
                throw new ArgumentNullException(nameof(studentDataAccess));
            }

            if (preferenceDataAccess == null)
            {
                throw new ArgumentNullException(nameof(preferenceDataAccess));
            }

            if (answerRatingDataAccess == null)
            {
                throw new ArgumentNullException(nameof(answerRatingDataAccess));
            }

            if (gradeDataAccess == null)
            {
                throw new ArgumentNullException(nameof(gradeDataAccess));
            }

            _studentDataAccess = studentDataAccess;
            _preferenceDataAccess = preferenceDataAccess;
            _answerRatingDataAccess = answerRatingDataAccess;
            _gradeDataAccess = gradeDataAccess;
        }

        public async Task<IList<RecommendedStudentDataTransfer>> GetRecommendedInstructorsForStudentAsync(
            StudentDataTransfer target)
        {
            var students = await _studentDataAccess.GetAllAsync();
            var allPreferences = await GetAllPreferencesForStudents(students);
            var othersPreferences = allPreferences.Where(p => p.Student.Id != target.Id).ToList();
            var recommender = new StudentRecommender(othersPreferences);
            var targetPreferences = allPreferences.Single(p => p.Student.Id == target.Id);

            var recommendedStudents = recommender
                .GetRecommenderStudentsForStudentPreferences(targetPreferences);

            var filteredRecommendedStudents = recommendedStudents
                .Where(s => s.Similarity >= targetPreferences.MinSimilarity / 100)
                .ToList();

            foreach (var student in filteredRecommendedStudents)
            {
                var ratingsCount = await _answerRatingDataAccess
                    .GetAllAsync(new Filter<AnswerRating>(r => r.Answer.Author.Id == student.Id));

                student.RatingsCount = ratingsCount.LongCount();
            }

            return filteredRecommendedStudents;
        }

        private async Task<ICollection<PreferenceDataTransfer>> GetAllPreferencesForStudents(
            IEnumerable<Student> students)
        {
            var studentsPreferences = new List<PreferenceDataTransfer>();

            foreach (var student in students)
            {
                var preferences = await _preferenceDataAccess.GetByStudentIdOrDefaultAsync(
                        student.Id);
                var preferencesDto = preferences.ToPreferenceDataTransfer();
                var tags = await _studentDataAccess.GetQuestionsTagsAsync(student.Id);
                var avgRating = await _studentDataAccess.GetAverageRatingAsync(student.Id);

                preferencesDto.Tags = tags.Select(t => t.ToTagAcceptedDataTransfer()).ToList();
                preferencesDto.MinRating = preferences.MinRating > 0 ? preferences.MinRating : 1f;
                preferencesDto.Student.Grade = student.Grade.ToGradeDataTransfer();
                preferencesDto.Student.GradeYear = (ushort)student.GradeYear;
                preferencesDto.Student.AvgRating = avgRating;
                preferencesDto.Student.Institution = student.Institution.ToInstitutionDataTransfer();

                studentsPreferences.Add(preferencesDto);
            }

            return studentsPreferences;
        }
    }
}

