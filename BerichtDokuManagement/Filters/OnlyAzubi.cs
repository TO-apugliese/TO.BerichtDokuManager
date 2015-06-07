using BerichtDokuManagement.Models;
using BerichtDokuManagement.Moduls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BerichtDokuManagement.Filters
{
    public class OnlyAzubi : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = SessionManager.GetSessionUser();
            if (user.UserRole == Enums.userType.Teamleiter.ToString())
            {
                // Redirect to login page
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary 
                    { 
                        { "controller", "TeamLeader" }, 
                        { "action", "AzubiReports" } 
                    });
            }
        }      
    }
}