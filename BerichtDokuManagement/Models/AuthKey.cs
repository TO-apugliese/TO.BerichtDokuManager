using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class AuthKey
    {
        #region Properties
        [Key]
        public int ID { get; set; }
        public string Key { get; set; }
        #endregion
    }
}