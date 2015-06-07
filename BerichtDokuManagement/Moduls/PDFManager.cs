using BerichtDokuManagement.Models;
using BerichtDokuManagement.Models.Template;
using Pechkin;
//using Pechkin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BerichtDokuManagement.Moduls
{
    public class PDFManager
    {
        #region Properties
        private static string _html { get; set; }
        #endregion

        #region Public functions
        public static string HtmlToPdf(string outputFilenamePrefix)
        {
            string pdfOutputPath = "/output/Pdf/";
            string htmlTemplatePath = System.Web.HttpContext.Current.Request.MapPath("/output/Html/PdfTemplate.html");
            string fullPdfOutputPath = System.Web.HttpContext.Current.Request.MapPath(pdfOutputPath);
            string outputFilename = outputFilenamePrefix + "_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".PDF";

            StreamReader reader = new StreamReader(htmlTemplatePath);
            string input = reader.ReadToEnd();
            reader.Close();

            byte[] pdfBuf = new SimplePechkin(new GlobalConfig()).Convert(input);
            System.IO.File.WriteAllBytes(fullPdfOutputPath + outputFilename, pdfBuf);

            return pdfOutputPath + outputFilename;
        }
        
        public static string RenderTemplate(string TemplateName, pdfTemplateInfo TempInfo)
        {
            var reader = new StreamReader(HttpContext.Current.Server.MapPath("/Templates") + TemplateName);
            _html = reader.ReadToEnd();

            var output = Render(TempInfo);

            return output;
        }
        #endregion

        #region Private functions
        private static string Render(pdfTemplateInfo values)
        {
            string output = _html;
            foreach (var p in values.GetType().GetProperties())
            {
                string type = null;
                string value = "wurde nicht gesetzt";
                if (p.GetValue(values) != null)
                {
                    type = p.GetValue(values).GetType().Name;
                    switch (type.ToLower())
                    {
                        case "datetime":
                            value = ((DateTime)(p.GetValue(values))).ToShortDateString();
                            break;
                        case "string":
                            value = p.GetValue(values) as string;
                            break;
                        default:
                            break;
                    }
                }

                output = output.Replace("[" + p.Name + "]", value);
            }
            return output;
        }
        #endregion
    }
}