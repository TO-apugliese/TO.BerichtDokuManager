using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using BerichtDokuManagement.Filters;
using BerichtDokuManagement.Models;
using BerichtDokuManagement.Moduls;
using System.Globalization;

namespace BerichtDokuManagement.Controllers
{
    [needAuthorization]
    public class WeekController : Controller
    {
        #region GET Actions

        [OnlyAzubi]
        public ActionResult AllWeeks()
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            // Laden aller Wochenberichte des eingelogten Users
            List<WeekReport> WeekReports = DBManager.Db.WeekReports.Where(x => x.UserID == user.ID).ToList();
           
            // Informationen fuer den Aufbau des Views
            ViewBag.hasWeekReports = WeekReports.Any();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(WeekReports);
        }

        [OnlyAzubi]
        [HttpGet]
        public ActionResult Create(int id)
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            //Laden des betroffenen Monats
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == monthReport.ID).ToList();
            DateTime beginDate = new DateTime(monthReport.ReportDate.Year,monthReport.ReportDate.Month,1);
            DateTime endDate = DateTime.DaysInMonth(beginDate.Year, beginDate.Month) >= (beginDate.Day + 4) ? new DateTime(beginDate.Year, beginDate.Month, (beginDate.Day + 4)) : new DateTime(beginDate.Year, beginDate.Month + 1, 1);
            int currentTrainingWeekCount = DBManager.Db.WeekReports.Where(x => x.UserID == user.ID).ToList().Sum(x => x.TrainingWeek);
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)monthReport.ReportDate).Month);

            bool hasWeekReports = monthReport.ListOfWeekReports.Any();

            if (hasWeekReports)
            {
                DateTime date = monthReport.ListOfWeekReports.Max(x => x.EndDate);
                beginDate = new DateTime(date.Year, date.Month, date.Day + 3);
                endDate = DateTime.DaysInMonth(beginDate.Year, beginDate.Month) >= (beginDate.Day + 4) ? new DateTime(beginDate.Year, beginDate.Month, (beginDate.Day + 4)) : new DateTime(beginDate.Year, beginDate.Month + 1, GetDiffer(DateTime.DaysInMonth(beginDate.Year, beginDate.Month), beginDate.Day + 4));
            }

            //Ermitteln der hoechsten Ausbildungswoche
            var hasTrainingsweeks = DBManager.Db.WeekReports.Where(x => x.UserID == user.ID).Any();
            var TrainingWeek = hasTrainingsweeks ? DBManager.Db.WeekReports.Where(x => x.UserID == user.ID).Select(x => x.TrainingWeek).Max() + 1 : 1;

            //Erstellen des neuen Wochenbericht Models
            WeekReport newWeekReport = new WeekReport()
            {
                hasPassing = false,
                isProceed = false,
                MonthReportID = id,
                UserID = user.ID,
                CompanyActivityEvaluation = "Hier folgt deine Teamleiter Bewertung",
                CompanyEducationEvaluation = "Hier folgt deine Teamleiter Bewertung",
                SchoolEducationEvaluation = "Hier folgt deine Teamleiter Bewertung",
                BeginDate = beginDate,
                EndDate = endDate,
                TrainingWeek = GetCalendarWeek(beginDate)
            };

            // Mit der DB abgleichen
            DBManager.Db.SaveChanges();

            //Informationen fuer den Aufbau des Views
            ViewBag.Title = "Du erstellst einen Wochenbericht im " + monthName + " " + monthReport.ReportDate.Year;

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(newWeekReport);
        }

        [HttpGet]
        public ActionResult Edit(int id, string referent = null)
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();
            
            //Laden der zu bearbeitenden Woche
            WeekReport weekReport = DBManager.Db.WeekReports.Single(x => x.ID == id);
            int reportYear = ((DateTime)DBManager.Db.MonthReport.Single(x => x.ID == weekReport.MonthReportID).ReportDate).Year;
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)DBManager.Db.MonthReport.Single(x => x.ID == weekReport.MonthReportID).ReportDate).Month);
            bool isAzubi = user.UserRole == Enums.userType.Auszubildender.ToString();

            if (user.UserRole == Enums.userType.Teamleiter.ToString())
            {
                weekReport.isProceed = true;    
            }

            //Informationen fuer den View
            ViewBag.Title = "Du " + (isAzubi ? "bearbeitest " : "bewertest ") + "momentan einen Wochenbericht von " + monthName + " " + reportYear;
            ViewBag.Referent = referent;
            ViewBag.Disabled = user.UserRole == Enums.userType.Teamleiter.ToString();
            ViewBag.userRole = user.UserRole;
            ViewBag.isAzubi = user.UserRole == Enums.userType.Auszubildender.ToString();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(weekReport);
        }

        [OnlyAzubi]
        [HttpGet]
        public ActionResult Delete(string referent, int id)
        {
            WeekReport weekReport = DBManager.Db.WeekReports.Single(x => x.ID == id);

            ViewBag.referent = referent;

            DBManager.Disconnect();
            return View(weekReport);
        }

        #endregion

        #region POST Actions

        [OnlyAzubi]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(WeekReport newWeekReport)
        {
            //Zuruecksetzen der bestehenden Wochenbericht PDFs
            // Laden des betroffenen Monatsberichts
            var monthReport = DBManager.Db.MonthReport.Single(x => x.ID == newWeekReport.MonthReportID);

            //Laden der zum Monat gehoerenden Wochenberichte
            //MonthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == newWeekReport.MonthReportID).ToList();

            //Monatsstatus auf bearbeitet setzen 
            SetMonthState(Enums.userType.Auszubildender.ToString(), newWeekReport.MonthReportID, Enums.reportState.bearbeitet.ToString());

            //neuen Wochenbericht fuer hinzufuegen markieren
            DBManager.Db.WeekReports.Add(newWeekReport);

            // Aktualisieren des Datums letzte Aenderungen
            UpdateLastChanges(monthReport);

            //Mit DB abgleichen
            DBManager.Db.SaveChanges();

            //DB Eintrag fuer kuenftig erzeugtes PDF anlegen
            CreatedPdf pdfRecord = new CreatedPdf()
            {
                CreatedDate = "",
                DownloadUrl = "",
                weekreport_ID = DBManager.Db.WeekReports.Max(x => x.ID),
            };

            //neues PDF fuer hinzufuegen markieren
            DBManager.Db.CreatedPdfRecords.Add(pdfRecord);
            DBManager.Db.SaveChanges();

            //Loeschen der schon bestehenden PDFs
            //foreach (var item in MonthReport.ListOfWeekReports)
            //{
            //    var DeleteRecordPDF = DBManager.Db.CreatedPdfRecords.Single(x => x.ID == item.ID);

            //    if (DeleteRecordPDF != null)
            //    {
            //        //Verschieben des PDFs nach output => old wenn eins exisitiert
            //        if (DeleteRecordPDF.DownloadUrl != "")
            //        {
            //            //PDF verschieben
            //            FileManager.Move(DeleteRecordPDF.DownloadUrl, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)MonthReport.ReportDate).Month));
            //        }

            //        //DB PDF Eintraege zuruecksetzen
            //        DeleteRecordPDF.DownloadUrl = string.Empty;
            //    }
            //}

            //Mit DB abgleichen
            //DBManager.Db.SaveChanges();
            DBManager.Disconnect();

            return RedirectToAction("Open", "Month", new { id = newWeekReport.MonthReportID });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(object referent, WeekReport modifiedModel)
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            // Laden des betroffenen Monats und sein Wochenberichte
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == modifiedModel.MonthReportID);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == monthReport.ID).ToList();

            WeekReport WeekReport = DBManager.Db.WeekReports.Single(x => x.ID == modifiedModel.ID);
            WeekReport WeeReportOrg = DBManager.Db.WeekReports.Single(x => x.ID == modifiedModel.ID);

            if (user.UserRole == Enums.userType.Auszubildender.ToString())
            {
                // Loeschen aller erzeugter PDFs
                DeletePdfRecords(null, monthReport.ListOfWeekReports);

                // Weekreport auf nicht mehr freigegeben setzen
                WeekReport.hasPassing = false;
            }

            //Monatsstatus auf bearbeitet setzen 
            SetMonthState(Enums.userType.Auszubildender.ToString(), modifiedModel.MonthReportID, Enums.reportState.bearbeitet.ToString());

            if (TryUpdateModel(WeekReport))
            {
                if (SessionManager.GetSessionUser().UserRole == Enums.userType.Auszubildender.ToString())
                {
                    modifiedModel.CompanyActivity = modifiedModel.CompanyActivity ?? string.Empty;
                    modifiedModel.CompanyEducation = modifiedModel.CompanyEducation ?? string.Empty;
                    modifiedModel.SchoolEducation = modifiedModel.SchoolEducation ?? string.Empty;
                    modifiedModel.CompanyActivity = modifiedModel.CompanyActivity.Replace("<br></div>", "</div><br>");
                    modifiedModel.CompanyEducation = modifiedModel.CompanyEducation.Replace("<br></div>", "</div><br>");
                    modifiedModel.SchoolEducation = modifiedModel.SchoolEducation.Replace("<br></div>", "</div><br>");
                    modifiedModel.CompanyActivityEvaluation = WeeReportOrg.CompanyActivityEvaluation;
                    modifiedModel.CompanyEducationEvaluation = WeeReportOrg.CompanyEducationEvaluation;
                    modifiedModel.SchoolEducationEvaluation = WeeReportOrg.SchoolEducationEvaluation;
                }
                else
                {
                    modifiedModel.CompanyActivity = WeeReportOrg.CompanyActivity;
                    modifiedModel.CompanyEducation = WeeReportOrg.CompanyEducation;
                    modifiedModel.SchoolEducation = WeeReportOrg.SchoolEducation;
                    modifiedModel.CompanyActivityEvaluation = modifiedModel.CompanyActivityEvaluation.Replace("<br></div>", "</div><br>");
                    modifiedModel.CompanyEducationEvaluation = modifiedModel.CompanyEducationEvaluation.Replace("<br></div>", "</div><br>");
                    modifiedModel.SchoolEducationEvaluation = modifiedModel.SchoolEducationEvaluation.Replace("<br></div>", "</div><br>");
                }

                // Aktualisierung der letzten Aenderung
                UpdateLastChanges(monthReport);

                // Mit DB abgleichen
                DBManager.Db.SaveChanges();
            }

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            //zurueck zur referenten Seite wenn user == Azubi
            if (user.UserRole == Enums.userType.Auszubildender.ToString())
            {
                //Pruefen ob Referent Monats- oder Wochen Uebersicht ist
                if (((string[])(referent))[0] == "Month")
                {
                    return RedirectToAction("Open", "Month", new { id = modifiedModel.MonthReportID });
                }
                return RedirectToAction("AllWeeks", "Week");
            }
            else
            {
                //Falls nicht Azubi sondern Teamleiter dann den betreffenden Monat oeffnen
                return RedirectToAction("Open", "Month", new { id = modifiedModel.MonthReportID });
            }

        }

        [OnlyAzubi]
        [HttpPost]
        public ActionResult Delete(WeekReport DeleteModel, string referent)
        {
            // Laden des betroffenen Monats
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == DeleteModel.MonthReportID);

            // Monatsstatus auf bearbeitet setzen 
            SetMonthState(Enums.userType.Auszubildender.ToString(), DeleteModel.MonthReportID, Enums.reportState.bearbeitet.ToString());

            DeletePdfRecords(DeleteModel,null);

            DBManager.Db.SaveChanges();

            //Laden der zu loeschenden Wochenberichte und der dazugehoerigen PDFs
            var weekReport = DBManager.Db.WeekReports.Single(x => x.ID == DeleteModel.ID);

            //Den zu loeschenden Wochenbericht markieren
            DBManager.Db.WeekReports.Remove(weekReport);

            //Mit DB abgleichen
            DBManager.Db.SaveChanges();

            //Wenn es keine Wochenberichte mehr gibt den Status hasPrintedPDFs auf false setzten
            var countOfMatchingsWeekReportsToMonth = DBManager.Db.WeekReports.Where(x => x.MonthReportID == weekReport.MonthReportID);
            if (countOfMatchingsWeekReportsToMonth.Any())
            {
                DBManager.Db.SaveChanges();
            }

            DBManager.Disconnect();

            ////Pruefen ob Referent Monats- oder Wochen Uebersicht ist
            if (referent == "Month")
            {
                return RedirectToAction("Open", "Month", new { id = DeleteModel.MonthReportID });
            }
            return RedirectToAction("AllWeeks", "Week");

        }

        #endregion

        #region Private functions
        public static int GetCalendarWeek(DateTime Datum)
        {
            CultureInfo CUI = CultureInfo.CurrentCulture;
            return CUI.Calendar.GetWeekOfYear(Datum, CUI.DateTimeFormat.CalendarWeekRule, CUI.DateTimeFormat.FirstDayOfWeek);
        }

        private int GetDiffer(int monthEnd, int days)
        {
            return days - monthEnd;
        }
        private void DeletePdfRecords(WeekReport weekreport = null, List<WeekReport> weekReports = null)
        {
            MonthReport monthReport;

            if (weekReports != null)
            {
                int monthID = weekReports.First().MonthReportID;
                monthReport = DBManager.Db.MonthReport.FirstOrDefault(x => x.ID == monthID);

                //Loeschen der erzeugten PDFs
                foreach (var item in weekReports)
                {
                    var DeleteRecordPDF = DBManager.Db.CreatedPdfRecords.Single(x => x.ID == item.ID);

                    //Verschieben des PDFs nach output => old wenn eins exisitiert
                    if (DeleteRecordPDF.DownloadUrl != "")
                    {
                        //PDF verschieben
                        FileManager.Move(DeleteRecordPDF.DownloadUrl, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)monthReport.ReportDate).Month));
                        DeleteRecordPDF.DownloadUrl = "";
                    }

                    // Mit DB abgleichen
                    DBManager.Db.SaveChanges();
                }
            }
            else if (weekreport != null)
            {
                monthReport = DBManager.Db.MonthReport.FirstOrDefault(x => x.ID == weekreport.MonthReportID);

                // Wenn PDF existiert dann loeschen
                var deletingPdfRecord = DBManager.Db.CreatedPdfRecords.FirstOrDefault(x => x.weekreport_ID == weekreport.ID);

                if (deletingPdfRecord != null)
                {
                    //Markieren zum loeschen
                    CreatedPdf createdPdf = DBManager.Db.CreatedPdfRecords.Remove(deletingPdfRecord);

                    //PDF in output => old verschieben falls dies erzeugt wurde
                    if (createdPdf.DownloadUrl != "")
                    {
                        FileManager.Move(createdPdf.DownloadUrl, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)monthReport.ReportDate).Month));
                    }
                }

                // Mit DB abgleichen
                DBManager.Db.SaveChanges();
            }
        }

        private void UpdateLastChanges(MonthReport pMonthReport)
        {
            pMonthReport.LastChanges = DateTime.Now;
        }

        private void SetMonthState(string affectedUserRole, int id, string state)
        {
            Users user = SessionManager.GetSessionUser();

            //Monatsstatus auf bearbeitet setzen 
            if (user.UserRole == affectedUserRole)
            {
                //Laden des betroffenen Monats
                MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);
                monthReport.Status = state;

                DBManager.Db.SaveChanges();
            }
        }
        #endregion
    }
}
