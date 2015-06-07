using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BerichtDokuManagement.Models.Template
{
    public class pdfTemplateInfo
    {
        public string IHKLogoSrc { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }
        public string Department { get; set; }
        public DateTime BeginingDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CompanyActivity { get; set; }
        public string CompanyActivityHours { get; set; }
        public string CompanyEducation { get; set; }
        public string CompanyEducationHours { get; set; }
        public string SchoolEducation { get; set; }
        public string SchoolEducationHours { get; set; }
    }
}