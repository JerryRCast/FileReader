using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LectorDePagos.Controller
{
    class TreatFile
    {
        private static int contador { get; set; }
        private static string DocNum = "";
        private static string nombre = "";
        private static string ruta = Config.resultPath;
        private static XmlDocument xml = new XmlDocument();
        /// <summary>
        /// Lee un archivo de complementos de pago para su posterior tratamiento
        /// </summary>
        /// <param name="file">Archivo a procesar</param>
        public static bool LeerArchivo(string archivo)
        {
            string pattern = "([0-9])\\w+";
            string linea = null;
            int numLinea = 0;
            int numLinea2 = 0;
            bool finalArchivo = false;
            string[] lineaSplit = null;
            string[] registro = new string[18];

            using (StreamReader reader = new StreamReader(archivo))
            {
                while (!finalArchivo)
                {
                    try
                    {
                        finalArchivo = (linea = reader.ReadLine()) == null;
                        if (finalArchivo)
                        {
                            Logger.WriteLog("\r\nLa bandera de [finalArchivo] ha cambiado de False a " +
                            finalArchivo + ". Se llego al final del archivo...");
                        }
                        else
                        {
                            if (linea.Contains("|"))
                            {
                                lineaSplit = linea.Split('|');
                                for (int i = 1; i < lineaSplit.Length - 1; i++)
                                {
                                    lineaSplit[i] = Regex.Replace(lineaSplit[i], " ", "");
                                    registro[i - 1] = lineaSplit[i];
                                }

                                if (Regex.IsMatch(registro[16], pattern))
                                {
                                    if (DocNum != registro[16])
                                    {
                                        DocNum = registro[16];
                                        nombre = registro[0] + "_" + registro[16];
                                        CrearXML(ruta + "\\" + nombre + ".xml", "ComplementoDePagos");
                                    }
                                    EscribirXML(registro, ruta + "\\" + nombre + ".xml", "ComplementoDePagos");
                                    numLinea2 += 1;
                                }
                            }
                        }
                        numLinea += 1;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog("Error: " + ex.Message);
                    }
                }
                Logger.WriteLog("Numero de lineas Leidas: " + numLinea + "\r\nNumero de lineas Escritas: " + numLinea2);
            }
            return finalArchivo;
        }

        /// <summary>
        /// Crea un nuevo documento Xml en la ruta completa dada y escribe el nodo raíz dado.
        /// </summary>
        /// <param name="rutaArchivo">Ruta del archivo Xml que se creará</param>
        /// <param name="nodoRaiz">Nodo raíz desde donde se comenzarán a agregar los diferentes nodos hijos</param>
        private static void CrearXML(string rutaArchivo, string nodoRaiz)
        {
            if (!File.Exists(rutaArchivo))
            {
                Logger.WriteLog("Creando Archivo: " + Path.GetFileName(rutaArchivo));
                XmlTextWriter writer = new XmlTextWriter(rutaArchivo, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement(nodoRaiz);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Crea un documento xml con los parametros pasados por la lectura del archivo txt original
        /// </summary>
        /// <param name="registro">nombre que tendrá el nuevo xml a crear</param>
        private static void EscribirXML(string[] registro, string rutaArchivo, string nodoRaiz)
        {
            xml.Load(rutaArchivo);
            XmlNode pago = CrearNodo(registro);
            XmlNode root = xml.DocumentElement;
            root.InsertAfter(pago, root.LastChild);
            xml.Save(rutaArchivo);
        }
        

        private static XmlNode CrearNodo(string[] reg)
        {
            string[] nombreAtributo = new string[] 
            {
                "Account", "Assignment", "Pmnt_date", "St", "Reference", "PBk", "Typ", "DocumentNo", "Doc..Date",
                "S", "DD", "Amount_in_DC", "Amt_in_loc_cur", "Curr", "LCu", "Text", "Clrng_doc", "User_Name"
            };
            XmlElement pago = xml.CreateElement("Pago");
            XmlAttribute attrib;
            int i = 0;
            foreach (var item in reg)
            {
                attrib = xml.CreateAttribute(nombreAtributo[i]);
                attrib.Value = reg[i];
                pago.SetAttributeNode(attrib);
                i += 1;
            }
            return pago;
        }
    }
}
