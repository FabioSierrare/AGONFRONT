using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class Pedidos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        [Display(Name = " Id del Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = " Estado del pedido")]
        public string Estado { get; set; }

        [Display(Name = " Total del pedido")]
        public decimal Total { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Display(Name = " Id del producto ")]
        public int ProductoId { get; set; }

        [Display(Name = " Id del vendedor")]
        public int VendedorId { get; set; }

        [Display(Name = " Cantidad del pedido")]
        public int Cantidad { get; set; }

        [Display(Name = " Metodo de Pago")]
        public string MetodoPago { get; set; }

        [Display(Name = " Precio del pedido")]
        public decimal PrecioUnitario { get; set; }

    }
}
