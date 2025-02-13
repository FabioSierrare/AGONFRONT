using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
    public class Envios
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("PedidoId")]
        [DisplayName("PedidoId")]
        public int PedidoId { get; set; }

        [DisplayName("EmpresaEnvio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string EmpresaEnvio { get; set; }

        [DisplayName("NumeroGuia")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NumeroGuia { get; set; }

        [DisplayName("EstadoEnvio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string EstadoEnvio { get; set; }

        [DisplayName("FechaEnvio")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEnvio { get; set; }

        [DisplayName("FechaEntrega")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }
    }
}
