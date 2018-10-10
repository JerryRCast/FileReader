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
            List<string[]> registros = new List<string[]>();

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
                                lineaSplit = AjustarArray(lineaSplit);

                                string val1 = lineaSplit[6];
                                if (Regex.IsMatch(lineaSplit[16], pattern))
                                {
                                    if (DocNum != lineaSplit[16])
                                    {
                                        DocNum = lineaSplit[16];
                                        if (registros.Count > 0) RevisarBloque(registros, false);
                                        registros.Clear();
                                    }
                                    registros.Add(lineaSplit);
                                    numLinea2 += 1;
                                }
                            }
                            else if (registros.Count > 0)
                            {
                                RevisarBloque(registros, false);
                                registros.Clear();
                            }
                        }
                        numLinea += 1;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog("Error: " + ex.Message);
                    }
                }
                Logger.WriteLog("Numero de lineas Leidas: " + numLinea + "\r\nNumero de lineas Procesadas: " + numLinea2);
            }
            return finalArchivo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineaSplit"></param>
        /// <returns></returns>
        private static string[] AjustarArray(string[] lineaSplit)
        {
            string[] registro = new string[18];
            for (int i = 1; i < lineaSplit.Length - 1; i++)
            {
                lineaSplit[i] = Regex.Replace(lineaSplit[i], " ", "");
                registro[i - 1] = lineaSplit[i];
            }
            return registro;
        }

        /// <summary>
        /// Revisa un bloque de registro con el mismo Numero de Documento (Clrng doc.) y evalua si debe o no crear un archivo para dicho bloque
        /// </summary>
        /// <param name="bloque">Lista con registros a revisar para la creación o no de un documento Xml</param>
        /// <param name="bandera">bandera para saber si crear un nuevo documento o solo añadirle datos a uno existente</param>
        private static void RevisarBloque(List<string[]> bloque, bool bandera)
        {
            foreach (var item in bloque)
            {
                nombre = "\\" + item[0] + "_" + item[16] + ".xml";
                switch (bandera)
                {
                    case true:
                        if (item[6] == "RV" || item[6] == "DR" || item[6] == "XD")
                        {
                            EscribirXML(item, ruta + nombre, true);
                        }
                        break;
                    case false:
                        if (item[6] == "DZ")
                        {
                            contador += 1;
                            CrearXML(ruta + nombre, "CFDPago");
                            EscribirXML(item, ruta + nombre, false);
                            RevisarBloque(bloque, true);
                        }
                        break;
                }
            } 
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
        private static void EscribirXML(string[] registro, string rutaArchivo, bool bandera)
        {
            xml.Load(rutaArchivo);
            XmlNode pago = CrearNodo(registro, bandera);
            XmlNode root = xml.DocumentElement;
            root.InsertAfter(pago, root.LastChild);
            xml.Save(rutaArchivo);
        }
        

        private static XmlNode CrearNodo(string[] reg, bool bandera)
        {
            XmlElement nodo = null;
            XmlAttribute attrib;
            string[] nombreAtributo;
            string[] atributo;
            int i = 0;

            switch (bandera)
            {
                case true: // Cuando se es RV, DR, o XD
                    nodo = xml.CreateElement("DoctoRelacionado");
                    nombreAtributo = new string[] { "Folio", "ImpPagado" };
                    atributo = new string[] { reg[4], Regex.Replace(reg[12],"([,-])","") };
                    foreach (var item in nombreAtributo)
                    {
                        attrib = xml.CreateAttribute(item);
                        attrib.Value = atributo[i];
                        nodo.SetAttributeNode(attrib);
                        i += 1;
                    }
                    i = 0;
                    break;
                case false: // Cuando se es DZ
                    string val = "";
                    if (reg[13] == "USD") val = OpTipoCambio(reg[11], reg[12]); 
                    nodo = xml.CreateElement("Pago");
                    nombreAtributo = new string[] { "Account", "FechaPago", "Monto", "MonedaP", "PaymentReference", "FormaDePagoP", "NumOperacion" };
                    atributo = new string[] { reg[0], reg[2], reg[11], reg[13], reg[4], "03", reg[16] };
                    foreach (var item in nombreAtributo)
                    {
                        attrib = xml.CreateAttribute(item);
                        attrib.Value = atributo[i];
                        nodo.SetAttributeNode(attrib);
                        i += 1;
                    }
                    i = 0;
                    break;
            }
            /*
            nombreAtributo = new string[] 
            {
                "Account", "Assignment", "Pmnt_date", "St", "Reference", "PBk", "Typ", "DocumentNo", "Doc..Date",
                "S", "DD", "Amount_in_DC", "Amt_in_loc_cur", "Curr", "LCu", "Text", "Clrng_doc", "User_Name"
            };
            */
            return nodo;
        }
        private static string OpTipoCambio(string moneda, string tipoCambio)
        {
            int mnac = Convert.ToInt32(moneda);
            int mint = Convert.ToInt32(tipoCambio);
            int result = mnac / mint;
            return result.ToString();
        }
    }
}
