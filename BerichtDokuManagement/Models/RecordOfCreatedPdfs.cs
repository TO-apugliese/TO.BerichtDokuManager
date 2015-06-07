using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class CreatedPdf
    {
        #region Properties
        [Key]
        public int ID { get; set; }

        public String DownloadUrl { get; set; }

        public String CreatedDate { get; set; }

        public int weekreport_ID { get; set; }
        #endregion
    }
}