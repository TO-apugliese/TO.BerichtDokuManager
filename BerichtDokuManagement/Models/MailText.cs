using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class MailText
    {
        #region Properties
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        #endregion
    }
}