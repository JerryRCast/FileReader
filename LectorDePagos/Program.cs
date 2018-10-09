using LectorDePagos.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorDePagos
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.ReadConfig();
            string path = Config.readPath;
            Logger.WriteLog("------------------------------------------------------------------------------------->>>");
            Logger.WriteLog("Proceso iniciado con fecha de " + DateTime.Now + "\r\nComenzando barrido de archivos...");
            //TreatFile.LeerArchivo(path);
            CheckDirectory.ObtenerArchivos(path);
            Logger.WriteLog("Terminando proceso.\r\n");
            //Console.Read();
        }
    }
}
