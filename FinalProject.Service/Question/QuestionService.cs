using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.Filters;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;
using FinalProject.Service.Notification;
using FinalProject.Shared.Expressions;

namespace FinalProject.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionDataAccess _questionDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionService(IQuestionDataAccess questionDataAccess, IUnitOfWork unitOfWork)
        {
            if (questionDataAccess == null)
            {
                throw new ArgumentNullException(nameof(questionDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _questionDataAccess = questionDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<QuestionDataTransfer> CreateQuestionAsync(QuestionDataTransfer questionDataTransfer)
        {
            if (questionDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var question = questionDataTransfer.ToQuestion();

            question = await _questionDataAccess.CreateAsync(question);
            await _unitOfWork.CommitAsync();

            var questionDto = question.ToQuestionDataTransfer();

            if (question.Status != QuestionStatus.Created)
            {
                return questionDto;
            }

            var notificationData = new NewQuestionNotificationData(questionDto);

            NotificationListener.Instance.NotifyCategoryListenersAsync(
                NotificationCategories.QuestionCreated,
                new NotificationEvent(notificationData));

            return questionDto;
        }

        public async Task<QuestionDataTransfer> GetQuestionByIdAsync(int id)
        {
            var question = await _questionDataAccess.GetByIdAsync(id);

            return question.ToQuestionDataTransfer();
        }

        public async Task<IList<QuestionDataTransfer>> GetAllQuestionsAsync()
        {
            var questions = await _questionDataAccess.GetAllAsync();

            return questions
                .Select(q => q.ToQuestionDataTransfer())
                .ToList();
        }

        public async Task<QuestionDataTransfer> UpdateQuestionAsync(QuestionDataTransfer questionDataTransfer)
        {
            if (questionDataTransfer == null)
            {
                throw new ArgumentNullException();
            }

            var question = questionDataTransfer.ToQuestion();

            question = await _questionDataAccess.UpdateAsync(question);
            await _unitOfWork.CommitAsync();
            return question.ToQuestionDataTransfer();
        }

        public async Task<IList<QuestionDataTransfer>> GetAllQuestionsForInstitutionAsync(
            int institutionId,
            IFilter<QuestionDataTransfer> filter, int maxItems = 10)
        {
            var questionFilter = filter?.ConvertFilter<QuestionDataTransfer, Question>();
            var questions = await _questionDataAccess.GetAllForInstitutionAsync(
                institutionId,
                questionFilter,
                maxItems);

            return questions
                .Select(q => q.ToQuestionDataTransfer())
                .ToList();
        }

        public async Task<IList<QuestionDataTransfer>> GetAllQuestionsForStudentAsync(
            int studentId,
            IFilter<QuestionDataTransfer> filter, int maxItems)
        {
            var questionFilter = filter?.ConvertFilter<QuestionDataTransfer, Question>();
            var questions = await _questionDataAccess.GetAllForStudentAsync(
                studentId,
                questionFilter,
                maxItems);

            return questions
                .Select(q => q.ToQuestionDataTransfer())
                .ToList();
        }

        public async Task<IList<QuestionDataTransfer>> GetAllQuestionsRelatedForStudentAsync(
            StudentDataTransfer student,
            IFilter<QuestionDataTransfer> filter,
            int maxItems)
        {
            var questionFilter = filter?.ConvertFilter<QuestionDataTransfer, Question>();
            var questions = await _questionDataAccess.GetAllForStudentAsync(
                student.Id,
                questionFilter,
                maxItems);

            var expressions = questions?.SelectMany(question => question.Tags.Select(tag =>
                (Expression<Func<Question, bool>>)(q =>
                    q.Tags.Any(t => t.Text.Equals(tag.Text, StringComparison.InvariantCultureIgnoreCase)))))
                    .ToList();

            var relatedQuestions = new List<Question>();

            if (expressions != null && expressions.Count > 0)
            {
                var studentQuestionsFilter = new Filter<Question>(expressions.Or());
                
                relatedQuestions.AddRange(await _questionDataAccess
                    .GetAllAsync(studentQuestionsFilter));
            }

            var studentTags = student.InstructorTags;

            if (studentTags == null || studentTags.Count == 0)
            {
                return relatedQuestions
                    .Select(q => q.ToQuestionDataTransfer())
                    .OrderByDescending(q => q.CreatedDate)
                    .ToList();
            }

            expressions = studentTags.Select(studentTag =>
                (Expression<Func<Question, bool>>)(q => q.Tags.Any(t => t.Id == studentTag.Id)))
            .ToList();

            var relatedQuestionsFilter = new Filter<Question>(expressions.Or());
            
            relatedQuestions.AddRange(await _questionDataAccess.GetAllAsync(relatedQuestionsFilter));

            return relatedQuestions
                .Where(q => q.Status == QuestionStatus.Created)
                .Distinct(new QuestionEqualityComparer())
                .Select(q => q.ToQuestionDataTransfer())
                .OrderByDescending(q => q.CreatedDate)
                .ToList();
        }

        public async Task<IList<QuestionDataTransfer>> GetAllContributedQuestionsForStudentAsync(
            int studentId, 
            IFilter<QuestionDataTransfer> filter, 
            int maxItems)
        {
            var questionFilter = filter?.ConvertFilter<QuestionDataTransfer, Question>();
            var expressions = new List<Expression<Func<Question,bool>>>
            {
                q => q.Author.Id == studentId || q.Answers.Any(a => a.Author.Id == studentId)
            };

            if (questionFilter != null)
            {
                expressions.AddRange(questionFilter.Expressions);
            }
            
            var answeredQuestions = await _questionDataAccess.GetAllAsync(new Filter<Question>(expressions.Or()));

            return answeredQuestions
                .Where(q => q.Status == QuestionStatus.Created)
                .Distinct(new QuestionEqualityComparer())
                .Select(q => q.ToQuestionDataTransfer())
                .OrderByDescending(q => q.CreatedDate)
                .ToList();
        }

        private class QuestionEqualityComparer : IEqualityComparer<Question>
        {
            public bool Equals(Question x, Question y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode(Question obj)
            {
                return obj.Id ^ obj.Title.GetHashCode();
            }
        }
    }
}