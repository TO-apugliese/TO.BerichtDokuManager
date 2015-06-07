using BerichtDokuManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BerichtDokuManagement.Moduls
{
    public class FileManager
    {
        #region Public functions

        #region File Access
        public static void Move(string deleteFilePath, string newFileName)
        {
            //Festlegen der zu bewegenden Datei und des neuen Pfades
            string sourceFile = HttpContext.Current.Server.MapPath(deleteFilePath);
            string destinationFile = HttpContext.Current.Server.MapPath("/output/Pdf/old/");
            string fullDestinationPath = destinationFile 
                                        + SessionManager.GetSessionUser().Username + "_"
                                        + newFileName 
                                        + DateTime.Now.Year + "-" + DateTime.Now.Month + "_"
                                        + RandomString()
                                        + ".pdf";

            // To move a file or folder to a new location:
            System.IO.File.Move(sourceFile, fullDestinationPath);
        }

        public static void CreateHtmlFile(string TextInput)
        {
            string savePath = HttpContext.Current.Server.MapPath("/output//Html") + "//PdfTemplate.html";

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(TextInput);
                }
            }
        }

        public static System.IO.FileInfo[] getAllTemplates(){
            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath("/Templates"));

            // Liste an Templates erstellen fuer Dropdown in View
            return ParentDirectory.GetFiles();
        }
        #endregion

        #region helper
        private static string RandomString()
        {
            System.Random rnd = new System.Random();
            StringBuilder Temp = new StringBuilder();
            for (Int64 i = 0; i < 6; i++)
            {
                Temp.Append(rnd.Next(0, 9));
            }
            return Temp.ToString();
        }
        #endregion

        #endregion
    }
}