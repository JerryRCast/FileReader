using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorDePagos.Controller
{
    class Config
    {
        public static string readPath { get; set; }
        public static string resultPath { get; set; }
        public static string logPath { get; set; }

        public static void ReadConfig()
        {
            // Extraemos todos los datos de Configuración y Seteamos
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            readPath = appSettings.Get("readPath");
            resultPath = appSettings.Get("resultPath");
            logPath = appSettings.Get("logPath");
        }
    }
}
