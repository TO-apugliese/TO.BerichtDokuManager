        using BerichtDokuManagement.Filters;
using BerichtDokuManagement.Models;
using BerichtDokuManagement.Moduls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BerichtDokuManagement.Controllers
{
    [OnlyTeamleader]
    [needAuthorization]
    public class TeamLeaderController : Controller
    {
        #region GET Actions
        [HttpGet]
        public ActionResult AzubiReports()
        {
            // Laden des eingelogten Users
            Users currentUser = SessionManager.GetSessionUser();

            //Laden aller  Nutzer des Teamleaders
            List<Users> users = DBManager.Db.Users.Where(x => x.TeamleiterID == currentUser.ID).ToList();

            //Laden aller Monatsberichte der Nutzer des Teamleader mit dem Status abgegeben
            List<MonthReport> monthReports = new List<MonthReport>();

            foreach (var user in users)
            {
                var CurrentUserMonthReports = DBManager.Db.MonthReport.Where(x => x.UserID == user.ID && x.Status == "abgegeben").ToList();
                foreach (var item in CurrentUserMonthReports)
	            {
                    monthReports.Add(item);
	            }
            }

            //Information fuer den Aufbau des Views
            ViewBag.AnyReport = monthReports.Any();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(monthReports);
        }

        [HttpGet]
        public ActionResult AzubiList() 
        {
            Users currentUser = SessionManager.GetSessionUser();

            //Laden aller Nutzer des Teamleaders
            List<Users> users = DBManager.Db.Users.Where(x => x.TeamleiterID == currentUser.ID).ToList();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(users);
        }

        [HttpGet]
        public ActionResult NewAzubis()
        {
            //Laden aller Azubis die keine Teamleader angehoeren
            List<Users> users = DBManager.Db.Users.Where(x => x.TeamleiterID == -1).ToList();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(users);
        }

        [HttpGet]
        public ActionResult Approval(int id)
        {
            //Laden des betroffenen Azubis
            Users user = DBManager.Db.Users.Single(x => x.ID == id);

            //Azubi als freigegeben markieren
            user.TeamleiterID = -1;

            //Mit DB abgleichen
            DBManager.Db.SaveChanges();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return RedirectToAction("AzubiList");
        }

        [HttpGet]
        public ActionResult AddToMyAzubis(int id)
        {
            Users currentUser = SessionManager.GetSessionUser();

            //Laden des betroffenen Users
            Users user = DBManager.Db.Users.Single(x => x.ID == id);

            //User als Azubi des Teamleiters markieren
            user.TeamleiterID = currentUser.ID;

            //Mit der DB abgleichen
            DBManager.Db.SaveChanges();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return RedirectToAction("NewAzubis");
        }
        #endregion
    }
}
