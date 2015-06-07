using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using BerichtDokuManagement.Moduls;

namespace BerichtDokuManagement.Models
{
    public class BerichtDokuManagementDB : DbContext
    {
        #region Properties
        public DbSet<MonthReport> MonthReport { get; set; }
        public DbSet<WeekReport> WeekReports { get; set; }
        public DbSet<CreatedPdf> CreatedPdfRecords { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserConfig> UserConfig { get; set; }
        public DbSet<MailText> MailText { get; set; }
        public DbSet<AuthKey> AuthKey { get; set; }
        public DbSet<Session> Session { get; set; }
        #endregion
    }

    public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<BerichtDokuManagementDB>
    {
        protected override void Seed(BerichtDokuManagementDB context)
        {
            context.MailText.Add(new MailText()
                {
                    Title = Enums.MailType.regisitrierung.ToString(),
                    Text = "Es hat sich der Azubi [azubiFullName] neu angemeldet."
                });
            
            context.MailText.Add(new MailText()
                {
                    Title = Enums.MailType.abgabe.ToString(),
                    Text = "[azubiFullName] hat das Berichtsheft von [reportMonth] [reportYear] abgegeben."
                });

            context.MailText.Add(new MailText()
            {
                Title = Enums.MailType.passwordReset.ToString(),
                Text = "Du hast die Passwort vergessen Funktion genutzt.<br /><br />  nachfolgend findest du dein Passwort: <br /> [userPassword]"
            });

            context.MailText.Add(new MailText()
            {
                Title = Enums.MailType.rueckgabe.ToString(),
                Text = "Dein Berichtsheft vom [reportMonth] [reportYear] wurde dir zurückgegeben."
            });

            context.AuthKey.Add(new AuthKey() 
            { 
                Key = AccountManager.SetPassword("qrCode")
            });

            context.SaveChanges();
        }
    }
}