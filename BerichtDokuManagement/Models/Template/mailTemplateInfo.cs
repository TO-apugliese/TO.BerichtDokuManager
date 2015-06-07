using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class MailInfo
    {
        public MailInfo()
        {
            this.RegisterName = null;
            this.MonthReport = null;
            this.user = null;
        }

        public Enums.MailType MailType { get; set; }
        public string RegisterName { get; set; }
        public MonthReport MonthReport { get; set; }
        public Users user { get; set; }

    }
}