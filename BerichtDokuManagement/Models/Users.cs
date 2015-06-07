using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class Users
    {
        #region Properties
        public int ID { get; set; }

        public string Anrede { get; set; }

        public String UserRole { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Mail { get; set; }

        public int TeamleiterID { get; set; }

        [ForeignKey("UserID")]
        public List<UserConfig> config { get; set; }
        #endregion
    }
}