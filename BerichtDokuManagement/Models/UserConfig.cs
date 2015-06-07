using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class UserConfig
    {
        #region Properties
        [Key]
        public int ID { get; set; }

        public string TemplatePath { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int UserID { get; set; }
        #endregion
    }
}