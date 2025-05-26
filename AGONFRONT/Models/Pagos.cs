using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class Pagos
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = "Epayco";
        public string CodigoTransaccion { get; set; } = "Null";
        public string ReferenciaPago { get; set; } = "Null";
        public string Factura { get; set; } 
        public string EstadoTransaccion { get; set; } = "pendiente_pago";
        public DateTime? FechaFinalizacionPago { get; set; }
    }
}
