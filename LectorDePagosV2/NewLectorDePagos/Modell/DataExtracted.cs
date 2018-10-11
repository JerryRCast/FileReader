using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewLectorDePagos.Modell
{
    class DataExtr
    {
        public string razonSoc { get; set; }
        public string rfc { get; set; }
    }

    class DataExtr2
    {
        public string remision { get; set; }
        public string moneda { get; set; }
        public Decimal tipoCambio { get; set; }
        public string formaPago { get; set; }
        public Decimal total { get; set; }
        public string uuid { get; set; }
        public double montoRem { get; set; }
    }

    
}
