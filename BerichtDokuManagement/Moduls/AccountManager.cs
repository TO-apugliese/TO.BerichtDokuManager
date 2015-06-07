using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Moduls
{
    public class AccountManager
    {
        #region User
        public static bool CreateUserAccount(RegisterModel model, Enums.userType pUserRole)
        {
            bool access = false;

            //Pruefen ob es eingegebenen Nutzernamen schon gibt
            bool UsernameExists = DBManager.Db.Users.Where(x => x.Username == model.UserName).Any();

            //Wenn dieser existiert Vorgang abbrechen
            if (UsernameExists) return access;

            var user = new Users() {
                Anrede = model.Anrede,
                Username = model.UserName,
                Password = Encode(model.Password),
                Mail = model.Mail,
                UserRole = pUserRole.ToString(),
                TeamleiterID = pUserRole == Enums.userType.Auszubildender ? -1 : 0
            };

            //Neuen Nutzer zum hinzufuegen markieren
            DBManager.Db.Users.Add(user);

            //Mit DB abgleichen
            DBManager.Db.SaveChanges();
            DBManager.Disconnect();

            //UserConfig erstellen
            CreateUserConfig(model.Vorname, model.Nachname);
            access = true;

            return access;
        }

        private static void CreateUserConfig(string pFirstName, string pLastName)
        {
            //Neue Nutzer Einstellung erstellen
            var userConfig = new UserConfig()
            {
                TemplatePath = FileManager.getAllTemplates().First().Name,
                FirstName = pFirstName,
                LastName = pLastName,
                UserID = DBManager.Db.Users.Max(x => x.ID),

            };

            //Neue Nutzer Einstellung zum hinzufuegen markieren
            DBManager.Db.UserConfig.Add(userConfig);

            //Mit DB abgleichen
            DBManager.Db.SaveChanges();
        }

        public static bool Login(string pUsername, string pPassword)
        {
            bool success = false;

            //Pruefen ob es angegebenen Benutzernamen gibt
            bool UsernameExists = DBManager.Db.Users.Where(x => x.Username == pUsername).Any();

            //Login einleiten falls dieser existiert
            if (UsernameExists)
            {
                //Laden des betroffnen Nutzers
                var user = DBManager.Db.Users.Single(x => x.Username == pUsername);

                //Pruefen ob eingegebenes Passwort dem Passwort in der DB entspricht
                success = Decode(user.Password) == pPassword;
                if (success)
                {
                    var sessionKey = SessionManager.CreateSession(user.ID);
                    HttpContext.Current.Session["CurrentUser"] = sessionKey; //Put value into session
                }
            }

            return success;
        }

        public static Users getUser(int id)
        {
            //Laden des gesuchten Nutzers
            Users user = DBManager.Db.Users.Single(x => x.ID == id) as Users;

            return user;
        }
        
        public static UserConfig getUserConfig(int id)
        {
            //Laden der gesuchten Nutzer einstellungen
            UserConfig userConfig = DBManager.Db.UserConfig.Single(x => x.UserID == id) as UserConfig;

            return userConfig;
        }
        #endregion

        #region Public functions
        public static string GetPassword(string password)
        {
            return Decode(password);
        }

        public static string SetPassword(string password)
        {
            return Encode(password);
        }
        #endregion

        #region Private functions
        private static string Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        private static string Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }
        #endregion
    }
}