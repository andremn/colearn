using System.Collections.Generic;
using Newtonsoft.Json;

namespace FinalProject.ViewModels
{
    public static class TagIconClass
    {
        public const string AprovedTagIconClass = "fa fa-thumbs-up";

        public const string ChildTagIconClass = "fa fa-tag";

        public const string InstitutionTagIconClass = "fa fa-graduation-cap";

        public const string PendingTagIconClass = "fa fa-clock-o";
    }

    public class TagItemViewModel
    {
        [JsonProperty("children")]
        public object Children { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class TagViewModel
    {
        public int Id { get; set; }
        
        public string Text { get; set; }
    }

    public class TagMovedNewParentViewModel
    {
        public string Id { get; set; }

        public string InstitutionId { get; set; }

        public string NewParentId { get; set; }

        public string OldParentId { get; set; }
    }

    public class PendingTagRequestsViewModel
    {
        public IList<TagRequestViewModel> Requests { get; set; }
    }

    public class TagRequestViewModel
    {
        public string Id { get; set; }

        public string Text { get; set; }
    }

    public class CreateOrUpdateTagViewModel
    {
        public string Id { get; set; }

        public string InstitutionId { get; set; }

        public string ParentId { get; set; }

        public string Text { get; set; }
    }

    public class MergeTagsViewModel
    {
        public string SourceId { get; set; }

        public string TargetId { get; set; }
    }
}