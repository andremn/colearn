using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using FinalProject.Helpers;
using FinalProject.MediaStream;

namespace FinalProject.Controllers.Api
{
    public class VideosController : ApiController
    {
        public HttpResponseMessage Get(long id)
        {
            var fileName = FileSystemHelper.GetAbsolutePath($"App_Data/Chats/{id}/{id}.mp4");
            var video = new VideoStream(fileName);
            var response = Request.CreateResponse();

            response.Content = new PushStreamContent(
                video.WriteToStreamAsync,
                new MediaTypeHeaderValue("video/mp4"));

            return response;
        }
    }
}