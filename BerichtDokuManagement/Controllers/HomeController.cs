using BerichtDokuManagement.Moduls;
using BerichtDokuManagement.Filters;
using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace BerichtDokuManagement.Controllers
{
    [needAuthorization]
    public class HomeController : Controller
    {
        #region GET Actions
        public ActionResult Index()
        {
            Users user = SessionManager.GetSessionUser();

            if (user.UserRole == Enums.userType.Auszubildender.ToString())
            {
                return RedirectToAction("AllMonths", "Month");
            }
            else if (user.UserRole == Enums.userType.Teamleiter.ToString())
            {
                return RedirectToAction("AzubiReports", "Teamleader");
            }
            return RedirectToAction("AllMonths", "Month");
        }
        #endregion
    }
}
