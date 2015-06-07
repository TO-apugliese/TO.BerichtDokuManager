using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class Session
    {
        #region Properties
        [Key]
        public int ID { get; set; }

        public string SessionKey { get; set; }

        public int UserID { get; set; }

        public DateTime TimeStamp { get; set; }
        #endregion
    }
}