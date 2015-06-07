using BerichtDokuManagement.Moduls;
using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BerichtDokuManagement.Filters
{
    public class needAuthorization : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
           var user = SessionManager.GetSessionUser();
           if (user == null)
           {
               // Redirect to login page
               filterContext.Result = new RedirectToRouteResult(
                   new RouteValueDictionary 
                    { 
                        { "controller", "Account" }, 
                        { "action", "Login" } 
                    });
           }
        }
    }
}