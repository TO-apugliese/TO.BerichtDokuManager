using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.IO;
using System.Net;
using System.Globalization;

namespace BerichtDokuManagement.Moduls
{
    public class MailManager
    {
        #region Properties
        private static string _html { get; set; }
        #endregion

        #region Public functions
        private static void SendMail(MailMessage mail)
        {
            SmtpClient client = new SmtpClient("mail.gmx.net", 587);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("a.pugliese@gmx.net", "acerHs244");
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;

            // versandt der E-Mail
            try
            {
                client.Send(mail);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void CreateNotificationMail(MailInfo mailInfo)
        {
            var file = new StreamReader(HttpContext.Current.Server.MapPath("/Templates/mailtemplate/template.html"));

            if (mailInfo.user == null)
            {
                Users user = SessionManager.GetSessionUser();
                mailInfo.user = user;
            }

            MailMessage mail = new MailMessage("a.pugliese@gmx.net", mailInfo.user.Mail);
            string mailContent = string.Empty;

            //DBManager.Connect();

            //Aufbau der E-Mail in Abhaengigkeit des Mail Types
            switch (mailInfo.MailType)
            {
                case Enums.MailType.regisitrierung:
                    mailContent = DBManager.Db.MailText.Single(x => x.Title == Enums.MailType.regisitrierung.ToString()).Text;
                    mail.Subject = "Bericht Doku Manager: " + Enums.MailType.regisitrierung.ToString();
                    break;
                case Enums.MailType.rueckgabe:
                    mailContent = DBManager.Db.MailText.Single(x => x.Title == Enums.MailType.rueckgabe.ToString()).Text;
                    mail.Subject = "Bericht Doku Manager: " + Enums.MailType.rueckgabe.ToString();
                    break;
                case Enums.MailType.abgabe:
                    mailContent = DBManager.Db.MailText.Single(x => x.Title == Enums.MailType.abgabe.ToString()).Text;
                    mail.Subject = "Bericht Doku Manager: " + Enums.MailType.abgabe.ToString();
                    break;
                case Enums.MailType.passwordReset:
                    mailContent = DBManager.Db.MailText.Single(x => x.Title == Enums.MailType.passwordReset.ToString()).Text;
                    mail.Subject = "Bericht Doku Manager: " + Enums.MailType.passwordReset.ToString();
                    break;
                default:
                    break;
            }

            var teamplate = CreatHtmlStringWithTemplate(file.ReadToEnd(), mailInfo, mailContent);
            file.Close();

            mail.Body = teamplate;

            // E-Mail absenden
            SendMail(mail);
        }
        #endregion

        #region Private functions
        private static string Render(object values)
        {
            string output = _html;
            foreach (var p in values.GetType().GetProperties())
                output = output.Replace("[" + p.Name + "]", p.GetValue(values, null) as string ?? "test");
            return output;
        }

        private static string CreatHtmlStringWithTemplate(string template, MailInfo mailInfo, string mailContent)
        {
            _html = template;
            var result = string.Empty;

            var password = AccountManager.GetPassword(mailInfo.user.Password);
            var azubiFullname = (mailInfo.MonthReport != null ? DBManager.Db.UserConfig.Single(x => x.UserID == mailInfo.MonthReport.UserID).FirstName : mailInfo.RegisterName)
                            + " "
                            + (mailInfo.MonthReport != null ? DBManager.Db.UserConfig.Single(x => x.UserID == mailInfo.MonthReport.UserID).LastName : "");
            var reportMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mailInfo.MonthReport != null ? ((DateTime)mailInfo.MonthReport.ReportDate).Day : 1);
            var reportYear = mailInfo.MonthReport != null ? ((DateTime)mailInfo.MonthReport.ReportDate).Year.ToString() : "1999";

            result = Render(new
            {
                content = mailContent,
                azubiFullName = azubiFullname,
                userPassword = password,
                reportMonth = reportMonth,
                reportYear = reportYear,
                salutation = mailInfo.user.Anrede == "Herr" ? "r " + mailInfo.user.Anrede : " ",
                firstName = DBManager.Db.UserConfig.Single(x => x.UserID == mailInfo.user.ID).FirstName,
                lastName = DBManager.Db.UserConfig.Single(x => x.UserID == mailInfo.user.ID).LastName
            });

            return result;
        }
        #endregion
    }
}