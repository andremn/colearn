using System.Web.Mvc;
using System.Web.Routing;

namespace FinalProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("HomePage", string.Empty, new { controller = "Account", action = "Login" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Account", action = "Index", id = UrlParameter.Optional});
        }
    }
}