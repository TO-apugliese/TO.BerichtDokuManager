using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Moduls
{
    public static class SessionManager
    {
        #region Public functions
        public static string CreateSession(int userID)
        {
            string sessionKey = Guid.NewGuid().ToString();

            Session newSession = new Session
            {
                SessionKey = sessionKey,
                TimeStamp = DateTime.Now,
                UserID = userID
            };

            DBManager.Db.Session.Add(newSession);
            DBManager.Db.SaveChanges();

            return sessionKey;
        }

        public static Session GetSession()
        {
            string sessionKey = HttpContext.Current.Session["CurrentUser"] as string;
            Session session = DBManager.Db.Session.FirstOrDefault(x => x.SessionKey == sessionKey);

            return session;
        }

        public static Users GetSessionUser()
        {
            string sessionKey = HttpContext.Current.Session["CurrentUser"] as string;
            Session session;
            Users user = null;

            if (sessionKey != null)
            {
                session = GetSession();
                if (session != null)
                {
                    user = DBManager.Db.Users.FirstOrDefault(x => x.ID == session.UserID) as Users;
                }
            }

            return user;
        }

        public static void RefreshSession()
        {
            string sessionKey = HttpContext.Current.Session["CurrentUser"] as string;
            Session session = DBManager.Db.Session.FirstOrDefault(x => x.SessionKey == sessionKey);

            if (session != null)
            {
                session.TimeStamp = DateTime.Now;
            }

            DBManager.Db.SaveChanges();
            DBManager.Disconnect();
        }

        public static void RemoveNotUsedSessions(int sessionLifeTime)
        {
            List<Session> sessions = null;
            object syncRoot = new Object();

            lock (syncRoot)
            {
                if (DBManager.Db.Session.Where(x => x.SessionKey != "") != null)
                {
                    sessions = DBManager.Db.Session.Where(x => x.SessionKey != "").ToList();
                }


                if (sessions != null)
                {
                    foreach (Session session in sessions)
                    {
                        if (session != null)
                        {
                            var differenz = (DateTime.Now - session.TimeStamp).TotalMinutes;

                            if (differenz >= sessionLifeTime)
                            {
                                SessionManager.RemoveSession(session);
                            }
                            else
                            {
                                SessionManager.RefreshSession();
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveSession(Session pSession = null)
        {
            string sessionKey = HttpContext.Current.Session["CurrentUser"] as string;
            HttpContext.Current.Session.Remove("CurrentUser");
            Session session = null;

            if (pSession == null)
            {
                session = DBManager.Db.Session.FirstOrDefault(x => x.SessionKey == sessionKey);
            }
            else
            {
                session = pSession;
            }


            if (session != null)
            {
                DBManager.Db.Session.Remove(session);
            }

            DBManager.Db.SaveChanges();
        }
        #endregion
    }
}