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
        public DateTime FechaPedido { get; set; }

    }
}
