using System;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace FinalProject.Common
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult(object data)
            : this(data, JsonRequestBehavior.DenyGet)
        {
        }
        public JsonNetResult(object data, JsonRequestBehavior behavior)
        {
            Data = data;
            JsonRequestBehavior = behavior;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);

            response.Write(serializedObject);
        }
    }
}