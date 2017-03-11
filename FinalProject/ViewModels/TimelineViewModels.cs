using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FinalProject.LocalResource;

namespace FinalProject.ViewModels
{
    public enum ItemsType
    {
        All = 0,
        Related = 1,
        Contribution = 2
    }

    public class TimelineItemViewModel
    {
        public string AnswersCount { get; set; }

        public string DateTime { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public string Institution { get; set; }

        public string Title { get; set; }

        public string User { get; set; }

        public int? UserId { get; set; }

        public string UserPicturePath { get; set; }

        public IList<string> Tags { get; set; }

        public int InstitutionId { get; set; }
    }

    public class TimelineViewModel
    {
        public IList<TimelineItemViewModel> Items { get; set; }
    }

    public class TimelineFilterViewModel
    {
        [Display(ResourceType = typeof(Resource), Name = "FilterTimelineTitleFieldPlaceholder")]
        public string Title { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "FilterTimelineTagsFieldPlaceholder")]
        public string[] Tags { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "FilterTimelineStudentItemsFieldLabel")]
        public bool ShowOnlyStudentItems { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "FilterTimelineStudentInstitutionItemsFieldLabel")]
        public bool ShowOnlyStudentInstitutionItems { get; set; }

        public ItemsType ItemsType { get; set; }

        public string Operator { get; set; }

        public int? MaxItems { get; set; }

        public int? LastId { get; set; }

        public int? UserId { get; set; }

        public IList<SelectListItem> Operators { get; set; }

        public IList<string> TagsList { get; set; }
    }
}