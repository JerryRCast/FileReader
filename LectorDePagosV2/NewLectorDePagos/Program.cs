using NewLectorDePagos.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NewLectorDePagos
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.ReadConfig();
            string path = Config.readPath;
            Logger.WriteLog("------------------------------------------------------------------------------------->>>");
            Logger.WriteLog("Proceso iniciado con fecha de " + DateTime.Now + "\r\nComenzando barrido de archivos...");
            CheckDirectory.ObtenerArchivos(path);
            Logger.WriteLog("Terminando proceso.\r\n");
        }
    }
}
