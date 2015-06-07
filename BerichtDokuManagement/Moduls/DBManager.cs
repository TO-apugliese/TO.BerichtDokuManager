using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Moduls
{
    public class DBManager
    {
        #region Properties
        private static BerichtDokuManagementDB db;
        private static object syncRoot = new Object();

        public static BerichtDokuManagementDB Db
        {
            get
            {
                if (db == null)
                {
                    lock (syncRoot)
                    {
                        if (db == null)
                            db = new BerichtDokuManagementDB();
                    }
                }
                return db;
            }
        }
        #endregion

        #region Public functions
        public static void Disconnect()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
        #endregion

        #region Private functions
        private static void Connect()
        {
            db = new BerichtDokuManagementDB();
        }
        #endregion
    }
}