using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using FinalProject.Common;
using FinalProject.DataAccess.Filters;
using FinalProject.Hubs;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Searching;
using FinalProject.Service;
using FinalProject.Service.Notification;
using FinalProject.ViewModels;
using Microsoft.AspNet.SignalR;

namespace FinalProject.Controllers
{
    [System.Web.Mvc.Authorize(Roles = UserRoles.SystemAdminRole + "," + UserRoles.InstitutionModeratorRole)]
    public class TagController : BaseController
    {
        private const string ApprovedTagsNodeId = "approved_";

        private const string PendingTagsId = "pendingTag_";

        private const string PendingTagsNodeId = "pending_";

        private const string RootTagNodeId = "root_";

        static TagController()
        {
            NotificationListener.Instance.AddListenerForCategory(
                NotificationCategories.NewTagsRequests, 
                OnTagNotification);
        }

        // POST: /Tag/CreateOrUpdateTag
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdateTag(CreateOrUpdateTagViewModel model)
        {
            var tagCreationErrorMessage = Resource.ErrorCreatingTagMessage;
            var tagService = GetService<ITagService>();
            var institutionService = GetService<IInstitutionService>();
            var tag = await tagService.GetTagByIdAsync(int.Parse(model.Id));
            var parent = await tagService.GetTagByIdAsync(ParseId(model.ParentId));
            var isNew = false;

            if (tag == null)
            {
                var institutionId = ParseId(model.InstitutionId);
                var institution = await institutionService.GetInstitutionByIdAsync(institutionId);

                if (institution == null)
                {
                    return CreateJsonResult(new { error = tagCreationErrorMessage }, false);
                }

                try
                {
                    tag = await tagService.CreateTagAsync(new TagAcceptedDataTransfer
                    {
                        Text = model.Text,
                        Institution = institution
                    });

                    if (parent == null)
                    {
                        return CreateJsonResult(new { error = tagCreationErrorMessage }, false);
                    }

                    parent.Children.Add(tag);
                    await tagService.UpdateTagAsync(parent);
                    isNew = true;
                }
                catch
                {
                    await tagService.DeleteTagAsync(tag);
                    throw;
                }
            }
            else
            {
                var institution = await FindInstitutionByTagAsync(tag.Id);

                if (institution == null)
                {
                    return CreateJsonResult(new { error = tagCreationErrorMessage }, false);
                }


                tag.Text = model.Text;
                tag = await tagService.UpdateTagAsync(tag);
            }

            return CreateJsonResult(new { id = tag.Id, text = tag.Text, isNew }, true);
        }

        // POST: /Tag/DeleteTag
        [HttpPost]
        public async Task<ActionResult> DeleteTag(int tagId, string parentId)
        {
            var tagService = GetService<ITagService>();
            var errorResponse = new { error = Resource.ErrorDeletingTagMessage };
            var institution = await FindInstitutionByTagAsync(tagId);

            if (institution == null)
            {
                return CreateJsonResult(errorResponse, false);
            }

            var tag = await tagService.GetTagByIdAsync(tagId);
            var tagToRemove = tag.Parents.SingleOrDefault(t => t.Id == ParseId(parentId));

            if (tagToRemove == null)
            {
                return CreateJsonResult(errorResponse, false);
            }

            tag.Parents.Remove(tagToRemove);
            await tagService.UpdateTagAsync(tag);

            return CreateJsonResult(null, true);
        }

        // GET: /Tag/Find
        [AllowAnonymous]
        public async Task<ActionResult> Find(string query, string addedTags)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);
            }

            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var institution = student.Institution;

            if (institution == null)
            {
                return HttpNotFound("Institution not found for the current student.");
            }

            var tags = await GetService<ITagService>()
                .GetAllTagsAsync(
                    new Filter<TagAcceptedDataTransfer>(
                        t => t.Text.Contains(query),
                        t => t.Institution.Id == institution.Id));

            var tagTexts = tags.Select(t => t.Text).ToList();

            if (string.IsNullOrWhiteSpace(addedTags))
            {
                return Json(tagTexts, JsonRequestBehavior.AllowGet);
            }

            var existingTags = addedTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            tagTexts = tagTexts.Except(existingTags).ToList();

            return Json(tagTexts, JsonRequestBehavior.AllowGet);
        }

        // GET: /Tag/GetTagsForInstitution
        public async Task<ActionResult> GetTagsForInstitution()
        {
            var tagService = GetService<ITagService>();
            var institutions = await GetInstitutionsForCurrentModeratorAsync();

            if (institutions == null)
            {
                return RedirectToAction("Index", "Timeline");
            }

            var tagsTree = new List<TagItemViewModel>();

            foreach (var institution in institutions)
            {
                var institutionTag =
                    await tagService.GetTagByNameForInstitutionAsync(
                        RootTagNodeId + institution.Id,
                        institution.Id) ??
                    await tagService.CreateTagAsync(new TagAcceptedDataTransfer
                    {
                        Text = RootTagNodeId + institution.Id,
                        Institution = institution
                    });

                var approvedTags = new TagItemViewModel
                {
                    Id = ApprovedTagsNodeId + institutionTag.Id,
                    Icon = TagIconClass.AprovedTagIconClass,
                    Text = Resource.ApprovedTagsNodeText
                };

                var pendingTags = new TagItemViewModel
                {
                    Id = PendingTagsNodeId + institutionTag.Id,
                    Icon = TagIconClass.PendingTagIconClass,
                    Text =
                        $"{Resource.PendingTagsNodeText} ({Resource.ApproveTagInstructionText})"
                };

                var tagViewModel = await ConvertToTagViewModelAsync(institutionTag, institution.Id, true);
                var children = tagViewModel.Children as List<TagItemViewModel>;

                tagViewModel.Text = institution.ShortName;

                if (children == null)
                {
                    return new JsonNetResult(tagViewModel, JsonRequestBehavior.AllowGet);
                }

                pendingTags.Children = await GetPendingTagsForInstitutionAsync(institution.Id);
                approvedTags.Children = children;
                tagViewModel.Children = new List<TagItemViewModel> { pendingTags, approvedTags };
                tagsTree.Add(tagViewModel);
            }

            return new JsonNetResult(tagsTree, JsonRequestBehavior.AllowGet);
        }

        // GET: /Tag/
        public ActionResult Index()
        {
            return View();
        }

        // POST: /Tag/MergeTags
        [HttpPost]
        public async Task<ActionResult> MergeTags(MergeTagsViewModel model)
        {
            var sourceId = ParseId(model.SourceId);
            var targetId = ParseId(model.TargetId);

            var tagService = GetService<ITagService>();
            var tagRequest = await tagService.GetTagRequestByIdAsync(sourceId);

            if (tagRequest == null)
            {
                return CreateJsonResult(new { error = Resource.ErrorNotPendingSourceTagMessage }, false);
            }

            var tag = await tagService.GetTagByIdAsync(targetId);

            if (tag == null)
            {
                return CreateJsonResult(new { error = Resource.ErrorNotFoundTargetTagMessage }, false);
            }

            if (tagRequest.Question != null)
            {
                await UpdateQuestionAsync(tagRequest.Question, tag);
            }

            tagRequest.Status = TagRequestStatus.Denied;
            await tagService.UpdateTagRequestAsync(tagRequest);

            return CreateJsonResult(null, true);
        }

        // POST: /Tag/MoveTagToNewParent
        [HttpPost]
        public async Task<ActionResult> MoveTagToNewParent(TagMovedNewParentViewModel model)
        {
            var isTagRequest = model.Id.StartsWith(PendingTagsId);

            if (isTagRequest)
            {
                return await HandleAcceptTagRequest(model);
            }

            return await HandleMoveTagRequest(model);
        }

        // POST: /Tag/RejectTag
        [HttpPost]
        public async Task<ActionResult> RejectTag(string tagId)
        {
            var tagService = GetService<ITagService>();
            var tagRequest = await tagService.GetTagRequestByIdAsync(ParseId(tagId));

            if (tagRequest == null)
            {
                return CreateJsonResult(new { error = Resource.ErrorTagNotFound }, false);
            }

            tagRequest.Status = TagRequestStatus.Denied;
            await tagService.UpdateTagRequestAsync(tagRequest);

            // Todo: notify student and update questions!
            return CreateJsonResult(null, true);
        }
        
        public async Task<ActionResult> GetTagRequestDetails(int id)
        {
            var tagRequest = await GetService<ITagService>().GetTagRequestByIdAsync(id);

            if (tagRequest == null)
            {
                return HttpNotFound($"Tag request with id {id} not found");
            }

            if (tagRequest.Question != null)
            {
                var question = tagRequest.Question;

                return Json(new
                {
                    source = "question",
                    data = new
                    {
                        id = question.Id,
                        title = question.Title
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            if (tagRequest.Author != null)
            {
                var author = tagRequest.Author;

                return Json(new
                {
                    source = "student",
                    data = new
                    {
                        id = author.Id,
                        name = $"{author.FirstName} {author.LastName}"
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        private static async Task<TagItemViewModel> ConvertToTagViewModelAsync(TagAcceptedDataTransfer tag, int institutionId, bool isRoot)
        {
            var tagViewModel = new TagItemViewModel
            {
                Text = tag.Text,
                Id = isRoot ? RootTagNodeId + institutionId : tag.Id.ToString(),
                Icon = isRoot
                        ? TagIconClass.InstitutionTagIconClass
                        : TagIconClass.ChildTagIconClass
            };

            if (tag.Children == null)
            {
                tag = await GetService<ITagService>().GetTagByIdAsync(tag.Id);
            }

            var tasksToWait = tag.Children.Select(t => ConvertToTagViewModelAsync(t, institutionId, false)).ToList();

            tagViewModel.Children = (await Task.WhenAll(tasksToWait)).ToList();

            return tagViewModel;
        }

        private static async Task<InstitutionDataTransfer> FindInstitutionByTagAsync(int tagId)
        {
            var tag = await GetService<ITagService>().GetTagByIdAsync(tagId);
            var rootTag = tag.Parents.FirstOrDefault(parent => parent.Parents == null || parent.Parents.Count == 0);

            if (rootTag == null)
            {
                return null;
            }

            return await GetService<IInstitutionService>().GetInstitutionByIdAsync(ParseId(rootTag.Text));
        }

        private static async Task<IList<TagItemViewModel>> GetPendingTagsForInstitutionAsync(int institutionId)
        {
            var pendingRequests = await GetService<ITagService>().GetAllTagsPendingForInstitutionAsync(institutionId);

            var pendingRequestsList = pendingRequests.Select(tag => new TagItemViewModel
            {
                Id = PendingTagsId + tag.Id,
                Text = tag.Text,
                Icon = TagIconClass.ChildTagIconClass,
                Children = false
            });

            return pendingRequestsList.ToList();
        }

        private static int ParseId(string id)
        {
            string stringToReplace;

            if (id.StartsWith(ApprovedTagsNodeId))
            {
                stringToReplace = ApprovedTagsNodeId;
            }
            else if (id.StartsWith(PendingTagsNodeId))
            {
                stringToReplace = PendingTagsNodeId;
            }
            else if (id.StartsWith(PendingTagsId))
            {
                stringToReplace = PendingTagsId;
            }
            else if (id.StartsWith(RootTagNodeId))
            {
                stringToReplace = RootTagNodeId;
            }
            else
            {
                return int.Parse(id);
            }

            return int.Parse(id.Replace(stringToReplace, string.Empty));
        }

        private static async Task UpdateInstructorTagsAsync(StudentDataTransfer student, TagAcceptedDataTransfer approvedTag)
        {
            var studentService = GetService<IStudentService>();

            student = await studentService.GetStudentByIdAsync(student.Id);
            student.InstructorTags.Add(approvedTag);

            await studentService.UpdateStudentAsync(student);
        }

        private static async Task UpdateQuestionAsync(QuestionDataTransfer question, TagAcceptedDataTransfer approvedTag)
        {
            var questionService = GetService<IQuestionService>();

            question = await questionService.GetQuestionByIdAsync(question.Id);

            if (question.Tags.Any(t => t.Text.Equals(approvedTag.Text, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            question.Tags.Add(approvedTag);

            if (question.Status != QuestionStatus.Created)
            {
                question.Status = QuestionStatus.Created;
                await QuestionSearcher.Default.IndexQuestionAsync(question.Id, question.Title);
            }

            await questionService.UpdateQuestionAsync(question);
        }

        private ActionResult CreateJsonResult(object data, bool success)
        {
            return Json(new { data, success });
        }

        private async Task<IList<InstitutionDataTransfer>> GetInstitutionsForCurrentModeratorAsync()
        {
            var currentModerator = await GetCurrentStudentAsync();

            return currentModerator?.ModeratingInstitutions;
        }

        private async Task<ActionResult> HandleAcceptTagRequest(TagMovedNewParentViewModel model)
        {
            var tagService = GetService<ITagService>();
            var tagRequestId = int.Parse(model.Id.Replace(PendingTagsId, string.Empty));
            var tagRequest = await tagService.GetTagRequestByIdAsync(tagRequestId);
            var error = new { error = Resource.ErrorUpdatingTagMessage };

            if (tagRequest == null)
            {
                return CreateJsonResult(error, false);
            }

            var parentId = ParseId(model.NewParentId);
            var parent = await tagService.GetTagByIdAsync(parentId);

            if (parent == null)
            {
                return CreateJsonResult(error, false);
            }

            parent.Children.Add(new TagAcceptedDataTransfer { Text = tagRequest.Text });

            parent = await tagService.UpdateTagAsync(parent);

            var institutionService = GetService<IInstitutionService>();
            var institutionId = ParseId(model.InstitutionId);
            var institution = await institutionService.GetInstitutionByIdAsync(institutionId);

            if (institution == null)
            {
                return CreateJsonResult(new { error }, false);
            }

            var approvedTag = parent.Children.SingleOrDefault(t => t.Text == tagRequest.Text);

            if (approvedTag == null)
            {
                return CreateJsonResult(error, false);
            }

            institution.Tags.Add(approvedTag);
            await institutionService.UpdateInstitutionAsync(institution);

            tagRequest.Status = TagRequestStatus.Approved;
            await tagService.UpdateTagRequestAsync(tagRequest);

            if (tagRequest.Question != null)
            {
                await UpdateQuestionAsync(tagRequest.Question, approvedTag);
            }

            if (tagRequest.Author != null)
            {
                await UpdateInstructorTagsAsync(tagRequest.Author, approvedTag);
            }

            return CreateJsonResult(new { id = approvedTag.Id }, true);
        }

        private async Task<ActionResult> HandleMoveTagRequest(TagMovedNewParentViewModel model)
        {
            var tagService = GetService<ITagService>();
            var affectedTag = await tagService.GetTagByIdAsync(int.Parse(model.Id));
            var error = new { error = Resource.ErrorUpdatingTagMessage };

            if (affectedTag == null)
            {
                return CreateJsonResult(error, false);
            }

            var parentId = ParseId(model.OldParentId);
            var oldParent = await tagService.GetTagByIdAsync(parentId);

            if (oldParent == null)
            {
                return CreateJsonResult(error, false);
            }

            var tagToRemove = oldParent.Children.SingleOrDefault(t => t.Id == affectedTag.Id);

            if (tagToRemove != null)
            {
                oldParent.Children.Remove(tagToRemove);
                await tagService.UpdateTagAsync(oldParent);
            }

            parentId = ParseId(model.NewParentId);

            var newParent = await tagService.GetTagByIdAsync(parentId);

            if (newParent == null)
            {
                return CreateJsonResult(error, false);
            }

            newParent.Children.Add(affectedTag);
            await tagService.UpdateTagAsync(newParent);

            return CreateJsonResult(null, true);
        }

        private static void OnTagNotification(IListener sender, NotificationEvent notificationEvent)
        {
            var notificationData = notificationEvent.Data as TagRequestNotificationData;

            if (notificationData == null)
            {
                return;
            }

            TagRequestHub.NotifyNewTagRequest(notificationData);
        }
    }
}