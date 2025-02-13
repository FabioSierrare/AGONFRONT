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
    public class DetallesPedidos
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("PedidoId")]
        [DisplayName("PedidoId")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        [DisplayName("PedidoId")]
        public int ProductoId { get; set; }

        [DisplayName("Cantidad")]
        [Required (ErrorMessage = "El campo {0} es requerido")]
        public int Cantidad { get; set; }

        [DisplayName("PrecioUnitario")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal PrecioUnitario { get; set; }
    }
}
