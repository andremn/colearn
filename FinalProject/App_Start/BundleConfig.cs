using System.Web.Optimization;

namespace FinalProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(
                new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*", "~/Scripts/validation.js"));

            bundles.Add(new ScriptBundle("~/bundles/select").Include("~/Scripts/bootstrap-select.js"));

            bundles.Add(new ScriptBundle("~/bundles/hubs").Include("~/signalr/hubs"));

            bundles.Add(new ScriptBundle("~/bundles/_layout").Include(
                "~/Scripts/_layout.js",
                "~/Scripts/Notification/notificationHandler.js"));

            bundles.Add(new ScriptBundle("~/bundles/_layout-institution").Include("~/Scripts/Tag/tagRequestHub.js"));

            bundles.Add(new ScriptBundle("~/bundles/_layout-student").Include("~/Scripts/Question/questionHub.js"));

            bundles.Add(new ScriptBundle("~/bundles/register").Include(
                "~/Scripts/Account/register.js",
                "~/Scripts/institutionSearchSelect.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/questionCreate").Include("~/Scripts/Question/create.js"));

            bundles.Add(new ScriptBundle("~/bundles/questionDetails").Include("~/Scripts/Question/details.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/learningMaterials").Include("~/Scripts/Question/learningMaterials.js"));

            bundles.Add(new ScriptBundle("~/bundles/timeline").Include(
                "~/Scripts/Timeline/timeline.js",
                "~/Scripts/Search/search.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/profile-edit").Include(
                    "~/Scripts/institutionSearchSelect.js",
                    "~/Scripts/Account/edit.js"));

            bundles.Add(new ScriptBundle("~/bundles/profile-details")
                .Include("~/Scripts/Account/details.js"));

            bundles.Add(new ScriptBundle("~/bundles/new-chat")
                .Include(
                    "~/Scripts/Question/newChat.js"));

            bundles.Add(
                new ScriptBundle("~/bundles/video-answer").Include(
                    "~/Scripts/WebRTC/screenshot.js",
                    "~/Scripts/WebRTC/adapter.js",
                    "~/Scripts/WebRTC/recordrtc.js",
                    "~/Scripts/WebRTC/peer.min.js",
                    "~/Scripts/jpeg-web-worker.js",
                    "~/Scripts/Question/videoWorker.js",
                    "~/Scripts/Question/audioWorker.js",
                    "~/Scripts/Question/videoAnswer.js"));

            bundles.Add(new ScriptBundle("~/bundles/agenda")
                .Include("~/Scripts/Agenda/agenda.js",
                         "~/Scripts/Agenda/humanize-duration.js"));

            bundles.Add(new ScriptBundle("~/bundles/preferences")
                .Include(
                    "~/Scripts/bootstrap-slider.min.js",
                    "~/Scripts/Search/preferences.js"));

            bundles.Add(new ScriptBundle("~/bundles/search")
                .Include("~/Scripts/Search/search.js"));

            bundles.Add(new ScriptBundle("~/bundles/grades")
                .Include("~/Scripts/Admin/grades.js",
                         "~/Scripts/Sortable/jquery.sortable.js",
                         "~/Scripts/Sortable/jquery.binding.js"));

            bundles.Add(new ScriptBundle("~/bundles/video-chat-rate")
                .Include("~/Scripts/VideoChat/rate.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(
                new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));

            bundles.Add(
                new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-paper.css",
                    "~/Content/custom-navbar.css",
                    "~/Content/site.css",
                    "~/Content/notification.css"));

            #region Views' content references

            bundles.Add(new StyleBundle("~/bundles/question-create-style").Include("~/Content/bootstrap-tagsinput.css"));

            bundles.Add(
                new StyleBundle("~/bundles/profile-edit-style").Include(
                    "~/Content/bootstrap-tagsinput.css",
                    "~/Content/ProfileEdit.css"));

            bundles.Add(
                new StyleBundle("~/bundles/timeline-style").Include(
                    "~/Content/text-ellipsis.css",
                    "~/Content/Timeline.css",
                    "~/Content/Search.css"));

            bundles.Add(new StyleBundle("~/bundles/spinner").Include("~/Content/Spinner.css"));

            bundles.Add(new StyleBundle("~/bundles/profile-style").Include("~/Content/Profile.css"));

            bundles.Add(new StyleBundle("~/bundles/question-details-style").Include("~/Content/QuestionDetails.css"));

            bundles.Add(new StyleBundle("~/bundles/new-chat-style").Include(
                "~/Content/Spinner.css",
                "~/Content/NewChat.css"));

            bundles.Add(
                new StyleBundle("~/bundles/video-answer-style").Include(
                    "~/Content/Spinner.css",
                    "~/Content/VideoAnswer.css"));

            bundles.Add(new StyleBundle("~/bundles/agenda-style")
                .Include(
                    "~/Content/Agenda.css",
                    "~/Content/fullcalendar.css"));

            bundles.Add(new StyleBundle("~/bundles/preferences-style")
                .Include(
                    "~/Content/bootstrap-slider.min.css",
                    "~/Content/Preferences.css"));

            bundles.Add(new StyleBundle("~/bundles/search-style")
                .Include(
                    "~/Content/bootstrap-slider.min.css",
                    "~/Content/Spinner.css",
                    "~/Content/Search.css"));

            bundles.Add(new StyleBundle("~/bundles/grades-style")
                .Include("~/Content/Grades.css"));

            bundles.Add(new StyleBundle("~/bundles/learning-materials-style")
                .Include("~/Content/LearningMaterials.css"));

            #endregion
        }
    }
}