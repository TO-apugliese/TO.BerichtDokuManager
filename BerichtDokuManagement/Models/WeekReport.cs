using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models
{
    public class WeekReport
    {
        #region Properties
        [Key]
        public int ID { get; set; }

        public String Department { get; set; }

        public int TrainingWeek { get; set; }

        public CreatedPdf CreatedPdf { get; set; }

        [Display(Name = "Ist OK (ankreuzen wenn es nichts zu beanstanden gibt)")]
        public bool hasPassing { get; set; }

        public bool isProceed { get; set; }

        public String CompanyActivity { get; set; }
        public String CompanyActivityEvaluation { get; set; }
        public int CompanyActivityHours { get; set; }

        public String CompanyEducation { get; set; }
        public String CompanyEducationEvaluation { get; set; }
        public int CompanyEducationHours { get; set; }

        public String SchoolEducation { get; set; }
        public String SchoolEducationEvaluation { get; set; }
        public int SchoolEducationHours { get; set; }

        public int MonthReportID { get; set; }

        public int UserID { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime BeginDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        #endregion
    }
}