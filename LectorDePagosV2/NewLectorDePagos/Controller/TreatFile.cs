using NewLectorDePagos.Modell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NewLectorDePagos.Controller
{
    class TreatFile
    {
        private static int contador = 0;
        private static string DocNum = "";
        private static string nombre = "";
        private static string ruta = Config.resultPath;
        private static double montoTotal = 0;
        private static double montoRem = 0;
        private static bool dz = false, gen = false;
        private static string uuid = "";
        

        /// <summary>
        /// Lee un archivo de complementos de pago para su posterior tratamiento
        /// </summary>
        /// <param name="archivo">Archivo a procesar</param>
        public static bool LeerArchivo(string archivo)
        {
            string pattern = "([0-9])\\w+";
            string linea = null;
            int numLinea = 0;
            int numLinea2 = 0;
            bool finalArchivo = false;
            string[] lineaSplit = null;
            TextModel reg = null;
            List<TextModel> registros = new List<TextModel>();

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
                                reg = AjustarArray(lineaSplit);

                                if (Regex.IsMatch(reg.clrngDoc, pattern))
                                {
                                    if (DocNum != reg.clrngDoc)
                                    {
                                        DocNum = reg.clrngDoc;
                                        if (registros.Count > 0) RevisarBloque(registros);
                                        registros.Clear();
                                    }
                                    registros.Add(reg);
                                    numLinea2 += 1;
                                }
                            }
                            else if (registros.Count > 0)
                            {
                                RevisarBloque(registros);
                                registros.Clear();
                            }
                        }
                        numLinea += 1;
                    }
                    catch (Exception ex) { Logger.WriteLog("--------------------->>>\r\nError: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
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
        private static TextModel AjustarArray(string[] regs)
        {
            string[] reg = new string[20];
            int i = 0;
            foreach (var item in regs)
            {
                reg[i] = Regex.Replace(item, " ", "");
                i += 1;
            }
            TextModel mod = new TextModel()
            {
                cliente = reg[1], assigment = reg[2], fechaP = reg[3], st = reg[4], referencia = reg[5], pbk = reg[6], tipoDoc = reg[7], numDoc = reg[8],
                fechaDoc = reg[9], s = reg[10], dd = reg[11], monto = reg[12], montoLoc = reg[13], tipoMoneda = reg[14], monedaLoc = reg[15], texto = reg[16],
                clrngDoc = reg[17], userName = reg[18]
            };
            return mod;
        }

        /// <summary>
        /// Revisa un bloque de registro con el mismo Numero de Documento (Clrng doc.) y evalua si debe o no crear un archivo para dicho bloque
        /// </summary>
        /// <param name="bloque">Lista con registros a revisar para la creación o no de un documento Xml</param>
        /// <param name="bandera">bandera para saber si crear un nuevo documento o solo añadirle datos a uno existente</param>
        private static void RevisarBloque(List<TextModel> bloque)
        {
            int count = bloque.Count - 1;
            List<TextModel> data = new List<TextModel>() { null };
            List<DataExtr2> xdata = new List<DataExtr2>();
            DataExtr2 remisionObj = new DataExtr2();
            DataExtr2 aux = new DataExtr2();
            TextModel tm = null;
            double acum = 0;
            bool dz = false, xd = false;

            for (int i = count; i >= 0; i--)
            {
                tm = bloque[i];
                nombre = string.Format("\\{0}_{1}_{2}.txt", tm.cliente, tm.clrngDoc, contador+1 );
                switch (tm.tipoDoc)
                {
                    case "DZ":
                    case "AB":
                        {
                            if (tm.tipoDoc == "DZ")
                            {
                                dz = true;
                                CrearTxt(ruta + nombre);
                                OpMontoTotal(tm.montoLoc, tm.monto);
                                data[0] = tm;
                            }
                            else if (tm.tipoDoc == "AB" && tm.montoLoc != "0.00")
                            {
                                if (dz)
                                {
                                    OpMontoTotal(tm.montoLoc, tm.monto);
                                }
                                else
                                {
                                    CrearTxt(ruta + nombre);
                                    OpMontoTotal(tm.montoLoc, tm.monto);
                                    data[0] = tm;
                                }
                            }
                        }
                        break;
                    case "RV":
                    case "DR":
                        if (data.Count >= 1) data.Add(tm);
                        break;
                    case "XD":
                        {
                            remisionObj = DBData.Extract3(tm.referencia);
                            if (uuid != remisionObj.uuid)
                            {
                                xd = true;
                                uuid = remisionObj.uuid;
                                montoRem = 0;
                                montoRem = montoRem + Convert.ToDouble(tm.montoLoc);
                            }
                            else montoRem = montoRem + Convert.ToDouble(tm.montoLoc);
                        }
                        break;
                }
                if (xd)
                {
                    aux.montoRem = acum;
                    xdata.Add(aux);
                    xd = false;
                }
                aux = remisionObj;
                acum = montoRem;
            }

            if (acum != 0 && !string.IsNullOrEmpty(aux.uuid))
            {
                aux.montoRem = acum;
                xdata.Add(aux);
            }

            if (File.Exists(ruta + nombre))
            {
                foreach (var item in data)
                {
                    if (item.tipoDoc == "DZ" || item.tipoDoc == "AB") EscribirTxt(item, ruta + nombre, "P01");
                    else if (item.tipoDoc == "RV" || item.tipoDoc == "DR") EscribirTxt(item, ruta + nombre, "DR1");
                }
                for(int i = 1; i < xdata.Count; i++)  EscribirTxt(xdata[i], ruta + nombre, "DR1");
            }
            contador += 1;
            data.Clear();
            xdata.Clear();
            aux = null;
            acum = 0;
            }

        /// <summary>
        /// Crea un nuevo documento Xml en la ruta completa dada y escribe el nodo raíz dado.
        /// </summary>
        /// <param name="rutaArchivo">Ruta del archivo Xml que se creará</param>
        /// <param name="nodoRaiz">Nodo raíz desde donde se comenzarán a agregar los diferentes nodos hijos</param>
        private static void CrearTxt(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                Logger.WriteLog("Creando Archivo: " + Path.GetFileName(rutaArchivo));
                StreamWriter sw = File.AppendText(rutaArchivo);
                sw.Close();
            }
        }

        private static void EscribirTxt(DataExtr2 reg, string rutaArchivo, string identificador)
        {
            string linea = identificador + "|";
            object[] atributo = new object[] { };
            Logger.WriteLog("Iniciando Proceso de Escritura");
            
            atributo = new object[] { reg.remision, reg.montoRem, reg.moneda, reg.tipoCambio, reg.formaPago, reg.total, reg.uuid };

            foreach (var item in atributo) linea = linea + item + "|";

            StreamWriter sw = File.AppendText(rutaArchivo);
            sw.Write(linea + "\r\n");
            sw.Close();
            linea = "";
            Logger.WriteLog("Terminando Proceso de Escritura");
        }
        /// <summary>
        /// Crea un documento xml con los parametros pasados por la lectura del archivo txt original
        /// </summary>
        /// <param name="registro">nombre que tendrá el nuevo xml a crear</param>
        private static void EscribirTxt(TextModel reg, string rutaArchivo,string identificador)
        {
            string linea = identificador + "|";
            object[] atributo = new object[] { };
            DataExtr info = new DataExtr();
            DataExtr2 otherData = new DataExtr2();
            Logger.WriteLog("Iniciando Proceso de Escritura");

            try
            {
                switch (identificador)
                {
                    case "P01":
                    string mnt = Math.Round(montoTotal, 2).ToString();
                    info = DBData.Extract(reg.cliente);
                    if (reg.monto == "USD")
                    {
                        string tipoCambio = OpTipoCambio(reg.monto, reg.montoLoc);
                        atributo = new string[] { reg.cliente, info.razonSoc, info.rfc, DateTime.Parse(reg.fechaP).ToString("yyyy-MM-dd"),
                            Regex.Replace(mnt, "([-])", ""), reg.tipoMoneda, reg.referencia, "03", reg.clrngDoc,
                            DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss"), tipoCambio };
                    }
                    else
                    {
                        atributo = new string[] { reg.cliente, info.razonSoc, info.rfc, DateTime.Parse(reg.fechaP).ToString("yyyy-MM-dd"),
                            Regex.Replace(mnt, "([-])", ""), reg.tipoMoneda, reg.referencia, "03", reg.clrngDoc, DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss"), "1" };
                    }
                    break;
                    case "DR1":
                        if (reg.tipoDoc == "RV")
                        {
                            otherData = DBData.Extract2(reg.referencia);
                            if (Regex.IsMatch(reg.montoLoc, "([,-])")) reg.montoLoc = Regex.Replace(reg.montoLoc, "([,-])", "");
                            double scan = Convert.ToDouble(otherData.total) - Convert.ToDouble(reg.montoLoc);

                            if (scan < 0) reg.montoLoc = otherData.total.ToString();
                            else if (scan > 0 && scan < 1) otherData.total = Convert.ToDecimal(reg.montoLoc);

                            atributo = new object[] { reg.referencia, reg.montoLoc, otherData.moneda, otherData.tipoCambio.ToString(),
                            otherData.formaPago, otherData.total.ToString(), otherData.uuid, "", "" };
                        }
                        else if (reg.tipoDoc == "DR")
                        {
                            otherData = DBData.Extract2(reg.numDoc);
                            if (Regex.IsMatch(reg.montoLoc, "([,-])")) reg.montoLoc= Regex.Replace(reg.montoLoc, "([,-])", "");
                            double scan = Convert.ToDouble(otherData.total) - Convert.ToDouble(reg.montoLoc);

                            if (scan < 0) reg.montoLoc = otherData.total.ToString();

                            atributo = new object[] { reg.numDoc, reg.montoLoc, otherData.moneda, otherData.tipoCambio.ToString(),
                            otherData.formaPago, otherData.total.ToString(), otherData.uuid, "", "" };
                        }
                    break;
                }
                montoTotal = 0;
            }
            catch (Exception ex) { Logger.WriteLog("--------------------->>>\r\nError: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }


            foreach (var item in atributo)
            {
                linea = linea + item + "|";
            }

            StreamWriter sw = File.AppendText(rutaArchivo);
            sw.Write(linea + "\r\n");
            sw.Close();
            linea = "";
            Logger.WriteLog("Terminando Proceso de Escritura");
        }

        private static string OpTipoCambio(string moneda, string tipoCambio)
        {
            int mint = Convert.ToInt32(moneda);
            int mnac = Convert.ToInt32(tipoCambio);
            int result = mint / mnac;
            return result.ToString();
        }

        private static void OpMontoTotal(string monto, string monto2)
        {
            double num = 0;
            if (Regex.IsMatch(monto, "([-])"))
            {
                num = Convert.ToDouble(Regex.Replace(monto2, "([,-])", ""));
                montoTotal = montoTotal - num;
            }
            else
            {
                num = Convert.ToDouble(Regex.Replace(monto2, "([,])", ""));
                montoTotal = montoTotal + num;
            }

        }
    }
}
