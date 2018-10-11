using NewLectorDePagos.Modell;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewLectorDePagos.Controller
{
    class DBData
    {
        private static string ConnString { get; set; }

        private static SqlConnection CreateConnection()
        {
            ConnString = "Server=" + Config.server + ";Database=" + Config.database + ";User Id=" + Config.user + ";Password=" + Config.pass + ";";
            SqlConnection conn = new SqlConnection(ConnString);
            return conn;
        }

        private static bool ValConn()
        {
            Logger.WriteLog("Iniciando prueba de conexión a la BD.\n");
            SqlConnection conn = CreateConnection();
            bool flag = false;
            try
            {
                Logger.WriteLog("Probando conexión...");
                conn.Open();
                Logger.WriteLog("Conexión exitosa!!!");
                flag = true;
                conn.Close();
                Logger.WriteLog("Cerrando conexión...");
            }
            catch (SqlException ex)
            { Logger.WriteLog("Error SQL: " + ex.Message + "\n" + ex.StackTrace); }
            catch (Exception ex)
            { Logger.WriteLog("Error: " + ex.Message + "\n" + ex.StackTrace); }
            return flag;
        }

        public static DataExtr Extract(string Account)
        {
            DataExtr data = new DataExtr();
            SqlDataReader reader;
            bool flag = ValConn();

            if (flag)
            {
                try
                {
                    Logger.WriteLog("Extrayendo Datos 1...");
                    SqlCommand cmd = new SqlCommand("select razonSocial, rfc from sociocomercial where socioComercialId = '" + Account + "';", CreateConnection());
                    cmd.Connection.Open();
                    cmd.CommandType = CommandType.Text;
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data = new DataExtr()
                            {
                                razonSoc = reader.GetString(0),
                                rfc = reader.GetString(1)
                            };
                        }
                    }
                    cmd.Connection.Close();
                    Logger.WriteLog("Terminando Primera Extracción...");
                    Logger.WriteLog("DATOS TRAIDOS DE LA BD: " + data.razonSoc + "," + data.rfc + "|END");
                }
                catch (SqlException ex)
                { Logger.WriteLog("--------------------->>>\r\nErrorSQL: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
                catch (Exception ex)
                { Logger.WriteLog("--------------------->>>\r\nError: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
            }
            else data = null;
            return data;
        }

        public static DataExtr2 Extract2(string NumDoc)
        {
            DataExtr2 data = new DataExtr2();
            SqlDataReader reader;
            bool flag = ValConn();

            if (flag)
            {
                try
                {
                    Logger.WriteLog("Extrayendo Datos 2...");
                    SqlCommand cmd = new SqlCommand("select cfdicomprobante.moneda," +
                        "cfdicomprobante.tipocambio,cfdicomprobante.formadepago, cfdicomprobante.total, cfditimbre.uuid from cfdicomprobante " +
                        "inner join cfditimbre on cfdicomprobante.nodocumento = cfditimbre.nodocumento where " +
                        "cfdicomprobante.nodocumento = '" + NumDoc + "';", CreateConnection());
                    cmd.Connection.Open();
                    cmd.CommandType = CommandType.Text;
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data = new DataExtr2()
                            {
                                moneda = reader.GetString(0),
                                tipoCambio = reader.GetDecimal(1),
                                formaPago = reader.GetString(2),
                                total = reader.GetDecimal(3),
                                uuid = reader.GetString(4)
                            };
                        }
                    }
                    cmd.Connection.Close();
                    Logger.WriteLog("Terminando Segunda Extracción...");
                    Logger.WriteLog("DATOS TRAIDOS DE LA BD: " + data.moneda + "," + data.tipoCambio + "," + data.formaPago + "," + data.total + "," + data.uuid + "|END");
                }
                catch (SqlException ex)
                { Logger.WriteLog("--------------------->>>\r\nErrorSQL: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
                catch (Exception ex)
                { Logger.WriteLog("--------------------->>>\r\nError: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
            }
            else data = null;
            return data;
        }

        public static DataExtr2 Extract3(string Reference)
        {
            DataExtr2 data = new DataExtr2();
            SqlDataReader reader;
            bool flag = ValConn();

            if (flag)
            {
                try
                {
                    Logger.WriteLog("Extrayendo Datos 3...");
                    SqlCommand cmd = new SqlCommand("select remision, uuid, tipocambio, FormaDePago, total, " +
                        "moneda from refacciones where remision = '" + Reference + "';", CreateConnection());
                    cmd.Connection.Open();
                    cmd.CommandType = CommandType.Text;
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data = new DataExtr2()
                            {
                                remision = reader.GetString(0),
                                uuid = reader.GetString(1),
                                tipoCambio = reader.GetDecimal(2),
                                formaPago = reader.GetString(3),
                                total = reader.GetDecimal(4),
                                moneda= reader.GetString(5)
                            };
                        }
                    }
                    cmd.Connection.Close();
                    Logger.WriteLog("Terminando Tercera Extracción...");
                    Logger.WriteLog("DATOS TRAIDOS DE LA BD: " + data.moneda + "," + data.tipoCambio + "," + data.formaPago + "," + data.total + "," + data.uuid + "|END");
                }
                catch (SqlException ex)
                { Logger.WriteLog("--------------------->>>\r\nErrorSQL: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
                catch (Exception ex)
                { Logger.WriteLog("--------------------->>>\r\nError: " + ex.Message + "\r\n" + ex.StackTrace + "\r\n--------------------->>>"); }
            }
            else data = null;
            return data;
        }
    }
}
