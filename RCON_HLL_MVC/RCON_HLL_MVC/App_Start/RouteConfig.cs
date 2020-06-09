using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RCON_HLL_MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //Adding RCON Area.
            routes.MapRoute(
                "RCON",                                           // Route name
                "Home",                            // URL with parameters
                new { controller = "RCON", action = "Home" }  // Parameter defaults
            );
        }
    }
}
