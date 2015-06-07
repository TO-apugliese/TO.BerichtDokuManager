using BerichtDokuManagement.Models;
using BerichtDokuManagement.Models.Template;
using BerichtDokuManagement.Moduls;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BerichtDokuManagement.Filters;

namespace BerichtDokuManagement.Controllers
{
    [needAuthorization]
    public class MonthController : Controller
    {
        #region GET Actions
        [OnlyAzubi]
        [HttpGet]
        public ActionResult AllMonths()
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            // Laden aller Monatsberichte des Users
            List<MonthReport> MonthReports = DBManager.Db.MonthReport.Where(x => x.UserID == user.ID).ToList();
            
            // Information fuer den Aufbau des Views
            ViewBag.MonthExist = MonthReports.Any();

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(MonthReports);
        }

        [HttpGet]
        public ActionResult Open(int id)
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            // Laden des geoeffneten Monats und der dazugehoerigen Wochenberichte und deren PDFs
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == id).ToList();
            monthReport.ListOfWeekReports.ForEach(x => x.CreatedPdf = DBManager.Db.CreatedPdfRecords.Single(y => y.weekreport_ID == x.ID));
            bool isAzubi = user.UserRole == Enums.userType.Auszubildender.ToString();
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthReport.ReportDate.Month);

            // Informationen für den Aufbau des Views ermitteln
            ViewBag.Title = "Du " + (isAzubi ? "bearbeitest " : "bewertest ") + monthName + " " + monthReport.ReportDate.Year;
            ViewBag.hasTeamleiter = user.TeamleiterID != -1;
            ViewBag.isAzubi = isAzubi;
            ViewBag.userRole = user.UserRole;
            ViewBag.MonthName = monthName;
            ViewBag.hasPrintedPdf = monthReport.ListOfWeekReports.Any(x => x.CreatedPdf.DownloadUrl != "");

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(monthReport);
        }

        [OnlyAzubi]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            //Laden des zu bearbeitenden Monats
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);

            // Setzen der Dropdownliste für den View
            ViewBag.MonatsListe = GetMonthList(monthReport.ReportDate.Month - 1);
            ViewBag.JahresListe = GetYearList(monthReport.ReportDate.Year);
            ViewBag.Title = 
                "Du bearbeitest " 
                + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthReport.ReportDate.Month) 
                + " " + monthReport.ReportDate.Year;

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(monthReport);
        }

        [OnlyAzubi]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            //Laden des zu loeschenden Monats
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);

            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return View(monthReport);
        }

        [OnlyAzubi]
        [HttpGet]
        public ActionResult Create()
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            //Setzen der Standartwerte fuer Monat und Jahr
            int maxYear = DateTime.Now.Year;
            int maxMonth = DateTime.Now.Month;

            //Ermitteln ob es Monatsreports gibt
            bool hasMonthReports = DBManager.Db.MonthReport.Where(x => x.UserID == user.ID).Any();

            //Ueberschreiben der Werte wenn es Monatsreports gibt
            if (hasMonthReports)
            {
                maxYear = DBManager.Db.MonthReport.Where(x => x.UserID == user.ID).Max(x => x.ReportDate.Year);
                maxMonth = DBManager.Db.MonthReport.Where(x => x.UserID == user.ID).Where(x => x.ReportDate.Year == maxYear).Max(x => x.ReportDate.Month) + 1;

                if (maxMonth == 13)
                {
                    maxYear += 1;
                    maxMonth = 1;
                }
            }

            // Ereugen des neue Monats fuer den View
            MonthReport newMonthReport = new MonthReport()
            {
                ReportDate = new DateTime(maxYear, maxMonth, 1),
                UserID = SessionManager.GetSessionUser().ID,
                Status = Enums.reportState.neu.ToString()
            };

            //Setzen der Listen fuer das Jahres und Monats Dropdown in den ViewBag
            ViewBag.MonatsListe = GetMonthList(maxMonth-1);
            ViewBag.JahresListe = GetYearList(maxYear);

            // Datenbank Verbindung trennen
            DBManager.Disconnect();

            return View(newMonthReport);
        }

        [HttpGet]
        public ActionResult Delivery(int id)
        {
            // Laden des eingelogten Users
            Users user = SessionManager.GetSessionUser();

            //Laden des betroffenen Monats und seine Wochenberichte
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == id).ToList();

            // Aendern des Monatsstatus abhaengig vom User
            if (user.UserRole == Enums.userType.Auszubildender.ToString())
            {
                //Den Status des betreffenden Monats auf abgegeben setzen
                SetMonthState(user.UserRole, monthReport, Enums.reportState.abgegeben.ToString());

                //Status von freigabe und bearbeitet des Berichts zuruecksetzen
                foreach (WeekReport weekreport in monthReport.ListOfWeekReports)
                {
                    if (!(weekreport.hasPassing))
                    {
                        weekreport.isProceed = false;
                    }
                }

                // Benachrichtigung des Teamleiters ueber Bericht Abgabe
                MailManager.CreateNotificationMail(new MailInfo {
                    MailType = Enums.MailType.abgabe,
                    MonthReport = monthReport,
                    user = DBManager.Db.Users.Single(x => x.ID == user.TeamleiterID),
                    RegisterName = null
                });

                // Aktualisieren der letzen Aenderung am Monatsbericht
                UpdateLastChanges(monthReport);

                //Mit der DB abgleichen
                DBManager.Db.SaveChanges();

                // Schließen der DB Verbindung
                DBManager.Disconnect();

                return RedirectToAction("Open", new { id = id });
            }
            else
            {
                //Liste aller Wochenberichte erstellen
                List<WeekReport> weekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == monthReport.ID).ToList();

                //Ueberpruefen ob alle Berichte freigegeben
                if (weekReports.Any(x => x.hasPassing == false))
                {
                    monthReport.Status = "korrektur";
                }
                else
                {
                    monthReport.Status = "freigabe";
                }

                //Mit Datenbank abgleichen
                DBManager.Db.SaveChanges();

                // Benachrichtigung des Auszubildenden ueber Bericht Rueckgabe
                MailManager.CreateNotificationMail(new MailInfo {
                    MailType = Enums.MailType.rueckgabe,
                    MonthReport =  monthReport,
                    user = DBManager.Db.Users.Single(x => x.ID == monthReport.UserID),
                    RegisterName = null
                });

                // Schließen der DB Verbindung
                DBManager.Disconnect();

                return RedirectToAction("AzubiReports", "TeamLeader");
            }
        }

        [HttpGet]
        [OnlyAzubi]
        public ActionResult Print(int id)
        {
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == id);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == id).ToList();
            Users user = SessionManager.GetSessionUser();
            UserConfig userConfig = DBManager.Db.UserConfig.Single(x => x.UserID == user.ID);

            //Von jedem Wochenbericht ein PDF erzeugen
            foreach (var item in monthReport.ListOfWeekReports)
            {
                //Laden aller Template Relevanten Daten
                pdfTemplateInfo TempInfo = new pdfTemplateInfo()
                {
                    IHKLogoSrc = string.Format("{0}{1}{2}:{3}{4}", new Object[] { Request.Url.Scheme, "://", Request.Url.Host, Request.Url.Port, "/Images/Logo.png", }),
                    Author = userConfig.FirstName + " " + userConfig.LastName,
                    Department = item.Department,
                    BeginingDate = item.BeginDate,
                    EndDate = item.EndDate,
                    Year = monthReport.ReportDate.Year.ToString(),
                    CompanyActivity = item.CompanyActivity != null ? item.CompanyActivity.Replace("\r\n", "<br/>") : "",
                    CompanyEducation = item.CompanyEducation != null ? item.CompanyEducation.Replace("\r\n", "<br/>") : "",
                    SchoolEducation = item.SchoolEducation != null ? item.SchoolEducation.Replace("\r\n", "<br/>") : "",
                    CompanyActivityHours = item.CompanyActivityHours.ToString(),
                    CompanyEducationHours = item.CompanyEducationHours.ToString(),
                    SchoolEducationHours = item.SchoolEducationHours.ToString()
                };

                // Laden und rendern des Templates 
                var htmlString = PDFManager.RenderTemplate("//" + userConfig.TemplatePath, TempInfo);

                // HTML-Datei Erzeugen für PDF-Erzeugung
                FileManager.CreateHtmlFile(htmlString);

                //Bearbeiten des PDF Records in der Datenbank
                var pdfRecord = DBManager.Db.CreatedPdfRecords.Single(x => x.weekreport_ID == item.ID);
                pdfRecord.CreatedDate = DateTime.Now.ToShortDateString();

                //Erzeugen des PDFs
                pdfRecord.DownloadUrl = PDFManager.HtmlToPdf(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)monthReport.ReportDate).Month));

                //speichern der PDF in der Datenbank
                DBManager.Db.SaveChanges();
            }

            DBManager.Disconnect();
            return RedirectToAction("Open", new { id = id });
        }
        #endregion

        #region POST Actions
        [OnlyAzubi]
        [HttpPost]
        public ActionResult Edit(int MonthNumber, int YearNumber, MonthReport modifiedModel)
        {
            //Laden des zu aendernden Monats
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == modifiedModel.ID);
            monthReport.ReportDate = new DateTime(YearNumber, MonthNumber, 1);

            //Updateversuch mit veraendertem Model
            if (TryUpdateModel(monthReport))
            {
                // Aktualisierung der letzten Aenderungen am Monat
                UpdateLastChanges(monthReport);

                // Abgleich mit der DB
                DBManager.Db.SaveChanges();

                // Monatsstatus auf bearbeitet setzen
                SetMonthState(Enums.userType.Auszubildender.ToString(), monthReport, Enums.reportState.bearbeitet.ToString());

                // Offene DB Verbindung schließen
                DBManager.Disconnect();

                return RedirectToAction("Open", new { id = modifiedModel.ID });
            }

            DBManager.Disconnect();
            return RedirectToAction("Edit", new { id = modifiedModel.ID });
        }

        [OnlyAzubi]
        [HttpPost]
        public ActionResult Delete(MonthReport deletingModel)
        {
            //Laden des zu loeschenden Monats und seiner Wochenberichte
            MonthReport monthReport = DBManager.Db.MonthReport.Single(x => x.ID == deletingModel.ID);
            monthReport.ListOfWeekReports = DBManager.Db.WeekReports.Where(x => x.MonthReportID == deletingModel.ID).ToList();

            // Alle zu Monat gehoerenden PDFs loeschen
            DeletePdfRecords(monthReport);

            //Monat zum loeschen aus der DB markieren
            DBManager.Db.MonthReport.Remove(monthReport);

            //Aenderungen mit der DB abgleichen
            DBManager.Db.SaveChanges();

            DBManager.Disconnect();
            return RedirectToAction("AllMonths");
        }

        [OnlyAzubi]
        [HttpPost]
        public ActionResult Create(int Month, int Year, MonthReport newMonthReport)
        {
            // Datum der letzten Aenderungen setzten
            UpdateLastChanges(newMonthReport);

            //Monatsbericht Datum setzten anhand der Dropdowns auswahl
            newMonthReport.ReportDate = new DateTime(Year, Month, 1);

            //Neuen Monatsbericht in der DB erzeugen
            DBManager.Db.MonthReport.Add(newMonthReport);

            //Mit der DB abgleichen
            DBManager.Db.SaveChanges();
            
            // Offene DB Verbindung schließen
            DBManager.Disconnect();

            return RedirectToAction("Open", new { id = newMonthReport.ID });
        }

        #endregion

        #region Private functions 
        private void SetMonthState(string affectedUserRole, MonthReport monthReport, string state)
        {
            Users user = SessionManager.GetSessionUser();

            //Monatsstatus auf bearbeitet setzen 
            if (user.UserRole == affectedUserRole)
            {
                //Setzten des Status
                monthReport.Status = state;

                DBManager.Db.SaveChanges();
            }
        }

        private void DeletePdfRecords(MonthReport monthReport) {

            //Loeschen der erzeugten PDFs
            foreach (var item in monthReport.ListOfWeekReports)
            {
                var DeleteRecordPDF = DBManager.Db.CreatedPdfRecords.Single(x => x.ID == item.ID);

                //Verschieben des PDFs nach output => old wenn eins exisitiert
                if (DeleteRecordPDF.DownloadUrl != "")
                {
                    //PDF verschieben
                    FileManager.Move(DeleteRecordPDF.DownloadUrl, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)monthReport.ReportDate).Month));
                }

                //PDF zum loeschen aus der DB markieren
                DBManager.Db.CreatedPdfRecords.Remove(DeleteRecordPDF);
            }
        }

        private void UpdateLastChanges(MonthReport pMonthReport) {
            pMonthReport.LastChanges = DateTime.Now;
        }

        private List<SelectListItem> GetMonthList(int maxMonth)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var months = Enum.GetValues(typeof(BerichtDokuManagement.Models.Enums.Month));
            
            for (int i = 0; i < months.Length; i++)
            {
                SelectListItem ListItem = new SelectListItem();
                ListItem.Text = months.GetValue(i).ToString();
                ListItem.Value = (i + 1).ToString();

                if (maxMonth == i)
                {
                    ListItem.Selected = true;
                }

                result.Add(ListItem);
            }

            return result;
        }

        private List<SelectListItem> GetYearList(int maxYear)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            int startYear = DateTime.Now.Year - 3;

            for (int i = 0; i < 9; i++)
            {
                SelectListItem ListItem = new SelectListItem();
                ListItem.Text = startYear.ToString();
                ListItem.Value = startYear.ToString();
                if (startYear == maxYear)
                {
                    ListItem.Selected = true;
                }
                result.Add(ListItem);
                startYear++;
            }

            return result;
        }
        #endregion
    }
}
