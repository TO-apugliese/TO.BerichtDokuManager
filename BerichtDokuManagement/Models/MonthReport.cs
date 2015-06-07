using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BerichtDokuManagement.Models
{
    public class MonthReport
    {
        #region Constructor
        public MonthReport()
        {
            ListOfWeekReports = new List<WeekReport>();
        }
        #endregion

        #region Properties
        [Key]
        public int ID { get; set; }
       
        public DateTime ReportDate { get; set; }
      
        [ForeignKey("MonthReportID")] 
        public List<WeekReport> ListOfWeekReports { get; set; }
        
        public string Status { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime LastChanges { get; set; }

        public int UserID { get; set; }
        #endregion
    }
}