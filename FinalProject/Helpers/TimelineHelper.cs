using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FinalProject.DataAccess.Filters;
using FinalProject.Extensions;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.ViewModels;

namespace FinalProject.Helpers
{
    public static class TimelineHelper
    {
        public static IFilter<QuestionDataTransfer> CreateFilter(TimelineFilterViewModel model)
        {
            Expression<Func<QuestionDataTransfer, bool>> resultExpression = null;
            var filter = new Filter<QuestionDataTransfer>();
            var expressions = new List<Expression<Func<QuestionDataTransfer, bool>>>();

            if (model.Tags?.Length > 0)
            {
                var tagExpressions = new List<Expression<Func<QuestionDataTransfer, bool>>>();

                tagExpressions.AddRange(model.Tags.Select(tag =>
                    (Expression<Func<QuestionDataTransfer, bool>>)(q =>
                        q.Tags.Any(t => t.Text.Equals(tag))))
                    .ToList());

                var tagsExpression = tagExpressions.Or();

                expressions.Add(tagsExpression);
            }

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                expressions.Add(q =>
                    q.Title.Contains(model.Title));
            }

            if (model.Operator.Equals(Resource.AndOperator,
                StringComparison.CurrentCultureIgnoreCase))
            {
                resultExpression = expressions.And();
            }
            else if (model.Operator.Equals(Resource.OrOperator,
                StringComparison.CurrentCultureIgnoreCase))
            {
                resultExpression = expressions.Or();
            }

            if (resultExpression != null)
            {
                filter.AddExpression(resultExpression);
            }

            return filter;
        }

        public static TimelineItemViewModel ConvertToTimelineItem(QuestionDataTransfer question)
        {
            return new TimelineItemViewModel
            {
                Id = question.Id,
                Institution = question.Institution.ShortName,
                User = $"{question.Author.FirstName} {question.Author.LastName}",
                UserId = question.Author.Id,
                Title = question.Title,
                DateTime = question.CreatedDate.ToLocalTime().ToShortDateString(),
                Description = question.Description,
                AnswersCount = GetAnswerText(question.Answers?.Count ?? 0),
                UserPicturePath = question.Author.GetProfilePicture(),
                Tags = question.Tags?.Select(t => t.Text).ToList(),
                InstitutionId = question.Institution.Id
            };
        }

        public static string GetAnswerText(int answersCount)
        {
            switch (answersCount)
            {
                case 0:
                    return Resource.NoAnswersText;
                case 1:
                    return Resource.SingleAnswerText;
                default:
                    return string.Format(Resource.MultipleAnswersText, answersCount);
            }
        }

        public static string GetFilterText(TimelineFilterViewModel model)
        {
            model.Tags = model.Tags ?? new string[0];

            if (model.Tags.Length > 0 &&
                !string.IsNullOrWhiteSpace(model.Title))
            {
                var tagText = GetTextForTags(model.Tags);
                var textFormat = model.Tags.Length == 1
                    ? Resource.TimelineFilterTextWithTitleTag
                    : Resource.TimelineFilterTextWithTitleTags;

                return string.Format(
                    textFormat,
                    model.Title,
                    model.Operator.ToLower(CultureInfo.CurrentCulture),
                    tagText);
            }

            if (model.Tags.Length > 0 &&
                string.IsNullOrWhiteSpace(model.Title))
            {
                var tagText = GetTextForTags(model.Tags);
                var textFormat = model.Tags.Length == 1
                    ? Resource.TimelineFilterTextWithTag
                    : Resource.TimelineFilterTextWithTags;

                return string.Format(
                    textFormat,
                    tagText);
            }

            if (model.Tags.Length == 0 &&
                !string.IsNullOrWhiteSpace(model.Title))
            {
                return string.Format(
                    Resource.TimelineFilterTextWithTitle,
                    model.Title);
            }

            return null;
        }

        private static string GetTextForTags(IReadOnlyList<string> tags)
        {
            if (tags.Count == 1)
            {
                return tags[0];
            }

            var tagText = new StringBuilder();
            int i;

            for (i = 0; i < tags.Count - 1; i++)
            {
                tagText.Append($@"""{tags[i]}""");

                if (i == tags.Count - 2)
                {
                    continue;
                }

                tagText.Append(", ");
            }

            tagText.Append($" {Resource.OrOperator.ToLower(CultureInfo.CurrentCulture)} ");
            tagText.Append($@"""{tags[i]}""");

            return tagText.ToString();
        }
    }
}