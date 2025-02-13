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
        [DisplayName("Id")]

        public int Id { get; set; }

        [ForeignKey("ClienteId")]
        [DisplayName("Clienteid")]
        public int ClienteId { get; set; }

        [DisplayName("Estado")]
        public string Estado { get; set; }

        [DisplayName("Total")]
        public decimal Total { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaPedido { get; set; }

    }
}
