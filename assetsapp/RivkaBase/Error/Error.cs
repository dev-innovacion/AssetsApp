using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Error
{
    /// <summary>
    ///     Error class used to handle the system errors
    /// </summary>
    public class Error
    {
        /// <summary>
        ///     Writes the error log into the ErrorLog file
        /// </summary>
        /// <param name="error">
        ///     The original Exception error
        /// </param>
        /// <param name="info">
        ///     More Info about the error as the module and controller
        /// </param>
        public static void Log(Exception error, string info)
        {
            string logTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ==> ";
            string path = HttpContext.Current.Server.MapPath("/Logs/");
            bool ready = Directory.Exists(path);

            if (!ready)
            {
                Directory.CreateDirectory(path);
            }
            
            StreamWriter sw = new StreamWriter(path + "ErrorLog", true);

            sw.WriteLine(logTime + "[" + info + "]" + " : " + error.Message);
            sw.Flush();
            sw.Close();
        }
    }

}
