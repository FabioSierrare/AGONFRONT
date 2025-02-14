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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Pedido")]
        public int PedidoId { get; set; }

        [Display(Name = " Empresa del Envio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string EmpresaEnvio { get; set; }

        [Display(Name = " Numero Guia")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NumeroGuia { get; set; }

        [Display(Name = " Estado del Envio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string EstadoEnvio { get; set; }

        [Display(Name = " Fecha del Envio")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEnvio { get; set; }

        [Display(Name = " Fecha de la Entrega")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaEntrega { get; set; }
    }
}
