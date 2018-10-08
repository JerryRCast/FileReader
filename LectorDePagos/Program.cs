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
            Logger.WriteLog("Comenzando lectura del archivo: " + Path.GetFileName(path));
            Logger.WriteLog("en la ruta " + Path.GetDirectoryName(path) + "\r\n");
            TreatFile.LeerArchivo(path);
            //Console.Read();
        }
    }
}
