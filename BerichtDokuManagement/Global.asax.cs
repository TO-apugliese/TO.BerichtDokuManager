using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using DotNetOpenAuth.AspNet;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BerichtDokuManagement.Filters;

namespace BerichtDokuManagement
{
    // Hinweis: Anweisungen zum Aktivieren des klassischen Modus von IIS6 oder IIS7 
    // finden Sie unter "http://go.microsoft.com/?LinkId=9394801".

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Register global filter
            GlobalFilters.Filters.Add(new RefreshSession());

            RegisterGlobalFilters(GlobalFilters.Filters);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            Database.SetInitializer<BerichtDokuManagementDB>(new DatabaseInitializer());
        }

        private void RegisterGlobalFilters(GlobalFilterCollection globalFilterCollection)
        {
            
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //** Here I have created a custom "Default" route that will route users to the "YourAction" method within the "YourNewController" controller.
            routes.MapRoute(
                name: "Default",
                url: "/",
                defaults: new { controller = "Month", Action = "AllMonths" }
            );
        }
    }
}