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
    public class RefreshSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Session session = SessionManager.GetSession();
            int sessionLifeTime = 15;

            SessionManager.RemoveNotUsedSessions(sessionLifeTime); 
        }
    }
}