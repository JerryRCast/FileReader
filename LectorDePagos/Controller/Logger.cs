using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorDePagos.Controller
{
    class Logger
    {
        public static void WriteLog(string msg)
        {
            string path = Config.logPath + "\\Log_LectorDePagos" + DateTime.Now.ToString("ddMMyyyy") + ".txt";
            try
            {
                StreamWriter sw = File.AppendText(path);
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError en WriteLog: " + ex.Message + "\nPila de llamadas: " + ex.StackTrace + "\n");
                Logger.WriteLog("\nError en WriteLog: " + ex.Message + "\nPila de llamadas: " + ex.StackTrace + "\n");
            }
        }
    }
}
