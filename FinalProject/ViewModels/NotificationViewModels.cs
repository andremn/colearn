using System.Collections.Generic;
using Newtonsoft.Json;

namespace FinalProject.ViewModels
{
    public class NotificationViewModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("actionUrl")]
        public string ActionUrl { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}