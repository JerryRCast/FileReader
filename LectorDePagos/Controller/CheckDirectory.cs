using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorDePagos.Controller
{
    class CheckDirectory
    {
        public static void ObtenerArchivos(string path)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path);
            FileInfo[] files = dinfo.GetFiles();
            bool archivoFinalizado;
            string finalPath = "";
            foreach (var file in files)
            {
                Logger.WriteLog("Comenzando lectura del archivo: " + file.Name);
                Logger.WriteLog("en la ruta " + file.DirectoryName + "\r\n");
                archivoFinalizado = TreatFile.LeerArchivo(file.FullName);
                if (archivoFinalizado)
                {
                    finalPath = path + "\\Ok\\" + file.Name;
                    file.MoveTo(finalPath);
                }
                else
                {
                    finalPath = path + "\\Error\\" + file.Name;
                    file.MoveTo(finalPath);
                }
            }
        }
    }
}
