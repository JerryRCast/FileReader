using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewLectorDePagos.Controller
{
    class Config
    {
        public static string readPath { get; set; }
        public static string resultPath { get; set; }
        public static string logPath { get; set; }
        public static string server { get; set; }
        public static string database { get; set; }
        public static string user { get; set; }
        public static string pass { get; set; }
        public static string consecutive { get; set; }

        public static void ReadConfig()
        {
            // Extraemos todos los datos de Configuración y Seteamos
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            readPath = appSettings.Get("readPath");
            resultPath = appSettings.Get("resultPath");
            logPath = appSettings.Get("logPath");
            server = appSettings.Get("Server");
            database = appSettings.Get("DB");
            user = appSettings.Get("User");
            pass = appSettings.Get("Pass");
            consecutive = appSettings.Get("Consecutive");
        }

        public static void UpdateConfig()
        {
            try
            {
                int newConsecutive = 0;//Writer.consecutive;
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Consecutive"].Value = Convert.ToString(newConsecutive);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar App.Config: " + ex.Message);
            }
        }
    }
}
