using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinalProject.LocalResource;

namespace FinalProject.ViewModels
{
    public class CreateQuestionViewModel
    {
        [Display(Name = "TypeQuestionDescriptionText", ResourceType = typeof(Resource))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "QuestionTagRequiredErrorMessage",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "ChooseTagsText", ResourceType = typeof(Resource))]
        public string Tags { get; set; }

        [Required(ErrorMessageResourceName = "QuestionTitleRequiredErrorMessage",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "TypeQuestionTitleText", ResourceType = typeof(Resource))]
        public string Title { get; set; }
        
        public int? AnswerId { get; set; }
    }

    public class LearningMaterialViewModel
    {
        public string Language { get; set; }

        public string Link { get; set; }

        public string Source { get; set; }

        public string Title { get; set; }
    }

    public class NewQuestionAnswerViewModel
    {
        public string AnswerText { get; set; }

        public int QuestionId { get; set; }
    }

    public class QuestionAnswerViewModel
    {
        public bool CanRate { get; set; }

        public int Count { get; set; }

        public string DateCreated { get; set; }

        public int Id { get; set; }

        public bool IsOwner { get; set; }

        public float? Rating { get; set; }

        public string Content { get; set; }

        public bool IsVideoAnswer { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string UserPicture { get; set; }

        public int QuestionId { get; set; }

        public string Thumbnail { get; set; }
    }

    public class QuestionDetailsViewModel
    {
        public bool CanAnswer { get; internal set; }

        public string DateCreated { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public bool IsOwner { get; set; }

        public IList<string> Tags { get; set; }

        public string Title { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string UserPicture { get; set; }
    }

    public class LearningMaterialsViewModel
    {
        public IList<LearningMaterialViewModel> Materials { get; set; }

        public uint Pages { get; set; }

        public uint CurrentPage { get; set; }

        public string Query { get; set; }
    }

    public class AnswerRatingViewModel
    {
        public int Id { get; set; }

        public float Value { get; set; }
    }

    public class VideoAnswerViewModel
    {
        public string PeerConnection { get; set; }

        public int StudentId { get; set; }
    }
}