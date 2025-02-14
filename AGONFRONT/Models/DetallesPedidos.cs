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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Pedido")]
        public int PedidoId { get; set; }

        [Display(Name = " Id del Producto")]
        public int ProductoId { get; set; }

        [Display(Name = " Cantidad del pedido")]
        [Required (ErrorMessage = " El campo {0} es requerido")]
        public int Cantidad { get; set; }

        [Display(Name = " Precio Unitario")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal PrecioUnitario { get; set; }
    }
}
