using System;
using System.Linq;

namespace FinalProject.Model
{
    public static class ModelExtensions
    {
        public static Question ToQuestion(this QuestionDataTransfer questionDataTransfer)
        {
            return new Question
            {
                Id = questionDataTransfer.Id,
                Title = questionDataTransfer.Title,
                Author = questionDataTransfer.Author?.ConvertStudentDataTransferToStudent(),
                Institution = questionDataTransfer.Institution?.ConvertInstitutionDataTransferToInstitution(),
                CreatedDate = questionDataTransfer.CreatedDate,
                Description = questionDataTransfer.Description,
                Status = questionDataTransfer.Status,
                Tags = questionDataTransfer.Tags?.Select(ConvertTagAcceptedDataTransferToTag).ToList(),
                Answers = questionDataTransfer.Answers?.Select(ConvertAnswerDataTransferToAnswer).ToList()
            };
        }

        public static Tag ToTag(this TagAcceptedDataTransfer tagAcceptedDataTransfer)
        {
            return new Tag
            {
                Children = tagAcceptedDataTransfer.Children?.Select(ConvertTagAcceptedDataTransferToTag).ToList(),
                Id = tagAcceptedDataTransfer.Id,
                Text = tagAcceptedDataTransfer.Text,
                Institution = tagAcceptedDataTransfer.Institution?.ConvertInstitutionDataTransferToInstitution(),
                Instructors = tagAcceptedDataTransfer.Instructors?.Select(ConvertStudentDataTransferToStudent).ToList(),
                Parents = tagAcceptedDataTransfer.Parents?.Select(ConvertTagAcceptedDataTransferToTag).ToList(),
                Questions = tagAcceptedDataTransfer.Questions?.Select(ConvertQuestionDataTransferToQuestion).ToList()
            };
        }

        public static TagRequest ToTagRequest(this TagRequestDataTransfer tagRequestDataTransfer)
        {
            return new TagRequest
            {
                Question = tagRequestDataTransfer.Question?.ConvertQuestionDataTransferToQuestion(),
                Id = tagRequestDataTransfer.Id,
                Text = tagRequestDataTransfer.Text,
                Author = tagRequestDataTransfer.Author?.ConvertStudentDataTransferToStudent(),
                Institution = tagRequestDataTransfer.Institution?.ConvertInstitutionDataTransferToInstitution(),
                Status = tagRequestDataTransfer.Status
            };
        }

        public static Preference ToPreference(this PreferenceDataTransfer preferenceDataTransfer)
        {
            return new Preference
            {
                Id = preferenceDataTransfer.Id,
                MinRating = preferenceDataTransfer.MinRating,
                MinSimilarity = preferenceDataTransfer.MinSimilarity,
                Grade = preferenceDataTransfer.Grade?.ToGrade(),
                Student = preferenceDataTransfer.Student?.ConvertStudentDataTransferToStudent(),
                Institutions = preferenceDataTransfer.Institutions?
                    .Select(ConvertInstitutionDataTransferToInstitution)
                    .ToList()
            };
        }

        public static Answer ToAnswer(this AnswerDataTransfer answerDataTransfer)
        {
            var answer = ConvertAnswerDataTransferToAnswer(answerDataTransfer);

            answer.Question =  answerDataTransfer.Question?
                .ConvertQuestionDataTransferToQuestion();

            answer.Author = answerDataTransfer.Author?
                .ConvertStudentDataTransferToStudent();

            answer.Ratings = answerDataTransfer.Ratings?
                .Select(ConvertAnswerRatingDataTransferToAnswerRating)
                .ToList();

            return answer;
        }

        public static Institution ToInstitution(this InstitutionDataTransfer institutionDataTransfer)
        {
            return new Institution
            {
                Id = institutionDataTransfer.Id,
                Code = institutionDataTransfer.Code,
                ShortName = institutionDataTransfer.ShortName,
                FullName = institutionDataTransfer.FullName,
                Questions = institutionDataTransfer.Questions?.Select(ConvertQuestionDataTransferToQuestion).ToList(),
                Tags = institutionDataTransfer.Tags?.Select(ConvertTagAcceptedDataTransferToTag).ToList()
            };
        }

        public static InstitutionRequest ToInstitutionRequest(
            this InstitutionRequestDataTransfer institutionRequestDataTransfer)
        {
            return new InstitutionRequest
            {
                Id = institutionRequestDataTransfer.Id,
                Status = institutionRequestDataTransfer.Status,
                CreatedTime = institutionRequestDataTransfer.CreatedTime,
                InstitutionCode = institutionRequestDataTransfer.InstitutionCode,
                InstitutionFullName = institutionRequestDataTransfer.InstitutionFullName,
                InstitutionShortName = institutionRequestDataTransfer.InstitutionShortName,
                OwnerEmail = institutionRequestDataTransfer.OwnerEmail
            };
        }

        public static Student ToStudent(this StudentDataTransfer studentDataTransfer)
        {
            return new Student
            {
                Id = studentDataTransfer.Id,
                Email = studentDataTransfer.Email,
                FirstName = studentDataTransfer.FirstName,
                IsModerator = studentDataTransfer.IsModerator,
                LastName = studentDataTransfer.LastName,
                ProfilePictureId = studentDataTransfer.ProfilePictureId,
                Grade = studentDataTransfer.Grade?.ToGrade(),
                GradeYear = (short)studentDataTransfer.GradeYear,
                Institution = studentDataTransfer.Institution?.ConvertInstitutionDataTransferToInstitution(),
                Questions = studentDataTransfer.Questions?.Select(ConvertQuestionDataTransferToQuestion).ToList(),
                ModeratingInstitutions =
                    studentDataTransfer.ModeratingInstitutions?.Select(ConvertInstitutionDataTransferToInstitution)
                        .ToList(),
                InstructorTags =
                    studentDataTransfer.InstructorTags?.Select(ConvertTagAcceptedDataTransferToTag).ToList()
            };
        }

        public static AnswerRating ToAnswerRating(this AnswerRatingDataTransfer studentDataTransfer)
        {
            return new AnswerRating
            {
                Value = studentDataTransfer.Value,
                Id = studentDataTransfer.Id,
                Author = studentDataTransfer.Author?.ConvertStudentDataTransferToStudent(),
                Answer = studentDataTransfer.Answer?.ConvertAnswerDataTransferToAnswer()
            };
        }

        public static Calendar ToCalendar(this CalendarDataTransfer calendarDataTransfer)
        {
            return new Calendar
            {
                Id = calendarDataTransfer.Id,
                FilePath = calendarDataTransfer.FilePath,
                Student = calendarDataTransfer.Student?.ConvertStudentDataTransferToStudent(),
                MaxScheduleTime = calendarDataTransfer.MaxScheduleTime
            };
        }
        public static PreferenceDataTransfer ToPreferenceDataTransfer(this Preference preference)
        {
            return new PreferenceDataTransfer
            {
                Id = preference.Id,
                MinRating = preference.MinRating,
                MinSimilarity = preference.MinSimilarity,
                Grade = preference.Grade?.ToGradeDataTransfer(),
                Student = preference.Student?.ConvertStudentToStudentDataTransfer(),
                Institutions = preference.Institutions?
                    .Select(ConvertInstitutionToInstitutionDataTransfer)
                    .ToList()
            };
        }

        public static CalendarDataTransfer ToCalendarDataTransfer(this Calendar calendar)
        {
            return new CalendarDataTransfer
            {
                Id = calendar.Id,
                FilePath = calendar.FilePath,
                Student = calendar.Student?.ConvertStudentToStudentDataTransfer(),
                MaxScheduleTime = calendar.MaxScheduleTime
            };
        }

        public static QuestionDataTransfer ToQuestionDataTransfer(this Question question)
        {
            return new QuestionDataTransfer
            {
                Title = question.Title,
                Id = question.Id,
                Tags = question.Tags?.Select(ConvertTagToTagAcceptedDataTransfer).ToList(),
                Author = question.Author?.ConvertStudentToStudentDataTransfer(),
                Description = question.Description,
                Status = question.Status,
                CreatedDate = question.CreatedDate,
                Institution = question.Institution?.ConvertInstitutionToInstitutionDataTransfer(),
                Answers = question.Answers?.Select(ConvertAnswerToAnswerDataTransfer).ToList()
            };
        }

        public static TagAcceptedDataTransfer ToTagAcceptedDataTransfer(this Tag tag)
        {
            return new TagAcceptedDataTransfer
            {
                Children = tag.Children?.Select(ConvertTagToTagAcceptedDataTransfer).ToList(),
                Id = tag.Id,
                Text = tag.Text,
                Institution = tag.Institution?.ConvertInstitutionToInstitutionDataTransfer(),
                Instructors = tag.Instructors?.Select(ConvertStudentToStudentDataTransfer).ToList(),
                Parents = tag.Parents?.Select(ConvertTagToTagAcceptedDataTransfer).ToList(),
                Questions = tag.Questions?.Select(ConvertQuestionToQuestionDataTransfer).ToList()
            };
        }

        public static TagRequestDataTransfer ToTagRequestDataTransfer(this TagRequest tagRequest)
        {
            return new TagRequestDataTransfer
            {
                Question = tagRequest.Question?.ConvertQuestionToQuestionDataTransfer(),
                Id = tagRequest.Id,
                Text = tagRequest.Text,
                Author = tagRequest.Author?.ConvertStudentToStudentDataTransfer(),
                Institution = tagRequest.Institution?.ConvertInstitutionToInstitutionDataTransfer(),
                Status = tagRequest.Status
            };
        }

        public static AnswerDataTransfer ToAnswerDataTransfer(this Answer answer)
        {
            var answerDataTransfer = ConvertAnswerToAnswerDataTransfer(answer);

            answerDataTransfer.Question = 
                answer.Question?
                .ConvertQuestionToQuestionDataTransfer();

            answerDataTransfer.Author = 
                answer.Author?
                .ConvertStudentToStudentDataTransfer();

            answerDataTransfer.Ratings = 
                answer.Ratings?
                .Select(ConvertAnswerRatingToAnswerRatingDataTransfer).ToList();

            return answerDataTransfer;
        }

        public static InstitutionDataTransfer ToInstitutionDataTransfer(this Institution institution)
        {
            return new InstitutionDataTransfer
            {
                Id = institution.Id,
                Tags = institution.Tags?.Select(ConvertTagToTagAcceptedDataTransfer).ToList(),
                Questions = institution.Questions?.Select(ConvertQuestionToQuestionDataTransfer).ToList(),
                Code = institution.Code,
                FullName = institution.FullName,
                Moderators = institution.Moderators?.Select(ConvertStudentToStudentDataTransfer).ToList(),
                ShortName = institution.ShortName
            };
        }

        public static InstitutionRequestDataTransfer ToInstitutionRequestDataTransfer(
            this InstitutionRequest institutionRequest)
        {
            return new InstitutionRequestDataTransfer
            {
                Id = institutionRequest.Id,
                Status = institutionRequest.Status,
                CreatedTime = institutionRequest.CreatedTime,
                InstitutionCode = institutionRequest.InstitutionCode,
                InstitutionFullName = institutionRequest.InstitutionFullName,
                InstitutionShortName = institutionRequest.InstitutionShortName,
                OwnerEmail = institutionRequest.OwnerEmail
            };
        }

        public static StudentDataTransfer ToStudentDataTransfer(this Student student)
        {
            return new StudentDataTransfer
            {
                Id = student.Id,
                Email = student.Email,
                Institution = student.Institution?.ConvertInstitutionToInstitutionDataTransfer(),
                Questions = student.Questions?.Select(ConvertQuestionToQuestionDataTransfer).ToList(),
                FirstName = student.FirstName,
                Grade = student.Grade.ToGradeDataTransfer(),
                GradeYear = (ushort)student.GradeYear,
                InstructorTags = student.InstructorTags?.Select(ConvertTagToTagAcceptedDataTransfer).ToList(),
                IsModerator = student.IsModerator,
                LastName = student.LastName,
                ModeratingInstitutions =
                    student.ModeratingInstitutions?.Select(ConvertInstitutionToInstitutionDataTransfer).ToList(),
                ProfilePictureId = student.ProfilePictureId
            };
        }

        public static AnswerRatingDataTransfer ToAnswerRatingDataTransfer(this AnswerRating answerRating)
        {
            return new AnswerRatingDataTransfer
            {
                Value = answerRating.Value,
                Id = answerRating.Id
            };
        }

        public static RecommendedStudentDataTransfer ToRecommendedStudentDataTransfer(
            this StudentDataTransfer studentDataTransfer)
        {
            return new RecommendedStudentDataTransfer
            {
                Email = studentDataTransfer.Email,
                Id = studentDataTransfer.Id,
                Institution = studentDataTransfer.Institution,
                FirstName = studentDataTransfer.FirstName,
                AvgRating = studentDataTransfer.AvgRating,
                InstructorTags = studentDataTransfer.InstructorTags,
                LastName = studentDataTransfer.LastName,
                ProfilePictureId = studentDataTransfer.ProfilePictureId,
                Grade = studentDataTransfer.Grade,
                GradeYear = studentDataTransfer.GradeYear,
                QuestionTags = studentDataTransfer.Questions?
                    .SelectMany(q => q.Tags)
                    .ToList()
            };
        }

        public static Grade ToGrade(this GradeDataTransfer gradeDataTransfer)
        {
            return new Grade
            {
                Id = gradeDataTransfer.Id,
                Name = gradeDataTransfer.Name,
                Order = gradeDataTransfer.Order
            };
        }

        public static GradeDataTransfer ToGradeDataTransfer(this Grade grade)
        {
            return new GradeDataTransfer
            {
                Id = grade.Id,
                Name = grade.Name,
                Order = grade.Order
            };
        }

        #region Helpers

        private static TagAcceptedDataTransfer ConvertTagToTagAcceptedDataTransfer(this Tag tag)
        {
            return new TagAcceptedDataTransfer
            {
                Id = tag.Id,
                Text = tag.Text
            };
        }
        
        private static AnswerDataTransfer ConvertAnswerToAnswerDataTransfer(this Answer answer)
        {
            var textAnswer = answer as TextAnswer;
            var videoAnswer = answer as VideoAnswer;
            AnswerDataTransfer answerDataTransfer = null;

            if (textAnswer != null)
            {
                answerDataTransfer = new TextAnswerDataTransfer
                {
                    Text = textAnswer.Text
                };
            }

            if (videoAnswer != null)
            {
                answerDataTransfer = new VideoAnswerDataTransfer
                {
                    VideoFilePath = videoAnswer.VideoPath
                };
            }

            if (answerDataTransfer == null)
            {
                throw new ArgumentException();
            }

            answerDataTransfer.Id = answer.Id;
            answerDataTransfer.CreatedDate = answer.CreatedDate;

            return answerDataTransfer;
        }

        private static StudentDataTransfer ConvertStudentToStudentDataTransfer(this Student student)
        {
            return new StudentDataTransfer
            {
                Id = student.Id,
                Email = student.Email,
                FirstName = student.FirstName,
                IsModerator = student.IsModerator,
                LastName = student.LastName,
                ProfilePictureId = student.ProfilePictureId,
                InstructorTags = student.InstructorTags?
                    .Select(ConvertTagToTagAcceptedDataTransfer)
                    .ToList()
            };
        }

        private static InstitutionDataTransfer ConvertInstitutionToInstitutionDataTransfer(
            this Institution institution)
        {
            return new InstitutionDataTransfer
            {
                Id = institution.Id,
                Code = institution.Code,
                ShortName = institution.ShortName,
                FullName = institution.FullName
            };
        }

        private static QuestionDataTransfer ConvertQuestionToQuestionDataTransfer(this Question question)
        {
            return new QuestionDataTransfer
            {
                Id = question.Id,
                CreatedDate = question.CreatedDate,
                Description = question.Description,
                Status = question.Status,
                Title = question.Title
            };
        }

        private static Question ConvertQuestionDataTransferToQuestion(this QuestionDataTransfer questionDataTransfer)
        {
            return new Question
            {
                Id = questionDataTransfer.Id,
                CreatedDate = questionDataTransfer.CreatedDate,
                Description = questionDataTransfer.Description,
                Status = questionDataTransfer.Status,
                Title = questionDataTransfer.Title
            };
        }

        private static Tag ConvertTagAcceptedDataTransferToTag(this TagAcceptedDataTransfer tagDataTransfer)
        {
            return new Tag
            {
                Id = tagDataTransfer.Id,
                Text = tagDataTransfer.Text
            };
        }

        private static Answer ConvertAnswerDataTransferToAnswer(this AnswerDataTransfer answerDataTransfer)
        {
            var textAnswerDataTransfer = answerDataTransfer as TextAnswerDataTransfer;
            var videoAnswerDataTransfer = answerDataTransfer as VideoAnswerDataTransfer;
            Answer answer = null;

            if (textAnswerDataTransfer != null)
            {
                answer = new TextAnswer
                {
                    Text = textAnswerDataTransfer.Text
                };
            }

            if (videoAnswerDataTransfer != null)
            {
                answer = new VideoAnswer
                {
                    VideoPath = videoAnswerDataTransfer.VideoFilePath
                };
            }

            if (answer == null)
            {
                throw new ArgumentException();
            }

            answer.Id = answerDataTransfer.Id;
            answer.CreatedDate = answerDataTransfer.CreatedDate;

            return answer;
        }

        private static Student ConvertStudentDataTransferToStudent(this StudentDataTransfer studentDataTransfer)
        {
            return new Student
            {
                Id = studentDataTransfer.Id,
                Email = studentDataTransfer.Email,
                FirstName = studentDataTransfer.FirstName,
                IsModerator = studentDataTransfer.IsModerator,
                LastName = studentDataTransfer.LastName,
                ProfilePictureId = studentDataTransfer.ProfilePictureId
            };
        }

        private static Institution ConvertInstitutionDataTransferToInstitution(
            this InstitutionDataTransfer institutionDataTransfer)
        {
            var i = new Institution
            {
                Id = institutionDataTransfer.Id,
                Code = institutionDataTransfer.Code,
                ShortName = institutionDataTransfer.ShortName,
                FullName = institutionDataTransfer.FullName
            };

            return i;
        }

        private static AnswerRatingDataTransfer ConvertAnswerRatingToAnswerRatingDataTransfer(
            this AnswerRating answerRating)
        {
            return new AnswerRatingDataTransfer
            {
                Author = answerRating.Author?.ConvertStudentToStudentDataTransfer(),
                Answer = answerRating.Answer?.ConvertAnswerToAnswerDataTransfer(),
                Value = answerRating.Value,
                Id = answerRating.Id
            };
        }

        private static AnswerRating ConvertAnswerRatingDataTransferToAnswerRating(
            this AnswerRatingDataTransfer answerDataTransfer)
        {
            return new AnswerRating
            {
                Author = answerDataTransfer.Author?.ConvertStudentDataTransferToStudent(),
                Answer = answerDataTransfer.Answer?.ToAnswer(),
                Value = answerDataTransfer.Value,
                Id = answerDataTransfer.Id
            };
        }

        #endregion
    }
}