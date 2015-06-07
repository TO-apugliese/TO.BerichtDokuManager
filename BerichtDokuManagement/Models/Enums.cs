using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    
    public class Enums
    {
        #region Enums
        public enum MailType
        {
            regisitrierung = 0,
            rueckgabe = 1,
            abgabe = 2,
            passwordReset = 3
        }

        public enum userType
        {
            Auszubildender = 0,
            Teamleiter = 1
        }

        public enum reportState { 
            neu = 0,
            bearbeitet = 1,
            Korrektur = 2,
            Freigabe = 3,
            abgegeben = 4
        }

        public enum Month
        {
            Januar,
            Februar,
            März,
            April,
            Mai,
            Juni,
            Juli,
            August,
            September,
            Oktober,
            November,
            Dezember
        };
    #endregion
    }
}