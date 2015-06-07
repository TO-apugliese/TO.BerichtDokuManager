using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using BerichtDokuManagement.Filters;
using BerichtDokuManagement.Models;
using BerichtDokuManagement.Moduls;
using DotNetOpenAuth.AspNet;
using System.IO;

namespace BerichtDokuManagement.Controllers
{
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        #region GET Actions
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl, string msg = null)
        {
            // Informationen fuer den Aufbau des Views
            ViewBag.UserNotValidInfo = msg;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RegisterTeamleiter(string msg = null)
        {
            // Informationen fuer den Aufbau des Views
            ViewBag.UserNotValidInfo = msg;

            ViewBag.Anrede = GetSalutationList();

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RegisterAzubi(string msg = null)
        {
            // Informationen fuer den Aufbau des Views
            ViewBag.UserNotValidInfo = msg;

            ViewBag.Anrede = GetSalutationList();

            return View();
        }

        [HttpGet]
        [ValidateInput(false)]
        public ActionResult Config(string msg)
        {
            // Laden des eingelogten Users und seiner Config
            Users user = SessionManager.GetSessionUser();
            UserConfig userConfig = DBManager.Db.UserConfig.Single(x => x.UserID == user.ID);

            // Informationen fuer den Aufbau des Views
            if (user.UserRole == Enums.userType.Teamleiter.ToString())
            {
                ViewBag.currentAuhtKey = AccountManager.GetPassword(DBManager.Db.AuthKey.First().Key);
            }

            // Informationen fuer den Aufbau des Views
            ViewData.Add("TemplateListe", GetTemplateList(userConfig.TemplatePath));
            ViewBag.SaveConfirmation = msg;
            ViewBag.userRole = user.UserRole;

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(userConfig);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string msg, bool done = false)
        {
            ViewBag.Info = done ? "done" : null;
            return View();
        }
        #endregion

        #region POST Actions
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var Successfull = false;
                while (!Successfull)
                {
                    Successfull = AccountManager.Login(model.UserName, model.Password);
                    if (!Successfull)
                    {
                        return RedirectToAction("Login", new { msg = "Benutzername oder Passwort sind ungültig" });
                    }
                }

                if (SessionManager.GetSessionUser().UserRole == Enums.userType.Auszubildender.ToString())
                {
                    return RedirectToAction("AllMonths", "Month");
                }
                else if (SessionManager.GetSessionUser().UserRole == Enums.userType.Teamleiter.ToString())
                {
                    return RedirectToAction("AzubiReports", "Teamleader");
                }
            }

            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
            ModelState.AddModelError("", "Der angegebene Benutzername oder das angegebene Kennwort ist ungültig.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterTeamleiter(RegisterModel model, string formAuthkey)
        {
            var currentAuthKey = AccountManager.GetPassword(DBManager.Db.AuthKey.First().Key);

            if (currentAuthKey == formAuthkey)
            {
                if (ModelState.IsValid)
                {
                    // Versuch, den Benutzer zu registrieren
                    try
                    {
                        var successfull = false;
                        while (!successfull)
                        {
                            successfull = AccountManager.CreateUserAccount(model, Enums.userType.Teamleiter);
                            if (!successfull)
                            {
                                return RedirectToAction("RegisterTeamleiter", new { msg = "Benutzername schon vergeben" });
                            }
                        }

                        AccountManager.Login(model.UserName, model.Password);
                        Users users = SessionManager.GetSessionUser();

                        DBManager.Disconnect();
                        return RedirectToAction("Index", "Home");
                    }
                    catch (MembershipCreateUserException e)
                    {
                        //ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                    }
                }
            }
            else
            {
                DBManager.Disconnect();
                return RedirectToAction("RegisterTeamleiter", new { msg = "Falscher BerechtigungsKey" });
            }

            DBManager.Disconnect();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAzubi(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Versuch, den Benutzer zu registrieren
                try
                {
                    var Successfull = false;
                    while (!Successfull)
                    {
                        Successfull = AccountManager.CreateUserAccount(model, Enums.userType.Auszubildender);
                        if (!Successfull)
                        {
                            return RedirectToAction("RegisterAzubi", new { msg = "Benutzername schon vergeben" });
                        }
                        else
                        {
                            var teamleaders = DBManager.Db.Users.Where(x => x.TeamleiterID == 0).ToList();

                            foreach (var teamleader in teamleaders)
                            {
                                // Benachrichtigung über Bericht Abgabe
                                MailManager.CreateNotificationMail( new MailInfo {
                                    MailType = Enums.MailType.regisitrierung,
                                    user = teamleader,
                                    RegisterName = model.Vorname + " " + model.Nachname,
                                    MonthReport = null
                                });
                            }
                        }
                    }

                    AccountManager.Login(model.UserName, model.Password);
                    Users users = SessionManager.GetSessionUser();

                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    //ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            DBManager.Disconnect();
            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
            return View(model);
        }

        [HttpPost]
        public ActionResult Config(UserConfig modifiedModel, string formAuthKey, string oldPassword, string newPassword, string repeatPassword)
        {
            Users user = SessionManager.GetSessionUser();
            UserConfig userConfig = DBManager.Db.UserConfig.Single(x => x.ID == modifiedModel.ID);
            string TempMsg = null;

            if (user.UserRole == Enums.userType.Teamleiter.ToString())
            {
                DBManager.Db.AuthKey.First().Key = AccountManager.SetPassword(formAuthKey);
            }

            if (newPassword == repeatPassword)
            {
                if (AccountManager.GetPassword(user.Password) == oldPassword)
                {
                    DBManager.Db.Users.Single(x => x.ID == user.ID).Password = AccountManager.SetPassword(newPassword);
                    DBManager.Db.SaveChanges();
                }
                else
                {
                    TempMsg = "Altes Passwort stimmt nicht!<br />";
                }
            }
            else
            {
                TempMsg = TempMsg == null ? "Neues Passwort ist ungleich<br />" : TempMsg + "Neues Passwort ist ungleich<br />";
            }

            if (TryUpdateModel(userConfig))
            {
                DBManager.Db.SaveChanges();
                TempMsg = TempMsg == null ? "Daten wurden gespeichert" : TempMsg + "Daten wurden gespeichert";

            }

            DBManager.Disconnect();
            return RedirectToAction("Config", new { msg = TempMsg });
        }

        [HttpPost]
        [needAuthorization]
        public ActionResult LogOut()
        {
            SessionManager.RemoveSession();

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(string username)
        {
            Users user = DBManager.Db.Users.FirstOrDefault(x => x.Username == username);
            var msg = string.Empty;

            if (user != null)
            {
                user.Password = AccountManager.SetPassword(Guid.NewGuid().ToString().Substring(0, 8));

                // Benachrichtigung über Bericht Abgabe
                MailManager.CreateNotificationMail(new MailInfo {
                    MailType = Enums.MailType.passwordReset,
                    user = user,
                    MonthReport = null,
                    RegisterName = null
                });
            }

            DBManager.Db.SaveChanges();
            DBManager.Disconnect();

            return RedirectToAction("ResetPassword", new { msg = msg, done = true });
        }
        #endregion

        #region Private functions
        private List<SelectListItem> GetSalutationList()
        {
            return new List<SelectListItem>() 
            { 
                new SelectListItem() { Text = "Herr", Value = "Herr" }, 
                new SelectListItem() { Text = "Frau", Value = "Frau" } 
            };
        }

        private List<SelectListItem> GetTemplateList(string selectedTemplate) {
            
            List<SelectListItem> result = new List<SelectListItem>();

            // Liste an Templates erstellen fuer Dropdown in View
            foreach (System.IO.FileInfo f in FileManager.getAllTemplates())
            {
                SelectListItem ListItem = new SelectListItem();
                ListItem.Text = Path.GetFileNameWithoutExtension(f.Name);
                ListItem.Value = f.Name;

                if (ListItem.Value == selectedTemplate)
                {
                    ListItem.Selected = true;
                }

                result.Add(ListItem);
            }

            return result;
        }
        #endregion
    }
}
