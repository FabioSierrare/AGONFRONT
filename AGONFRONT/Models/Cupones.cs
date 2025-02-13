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
    public class Cupones
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("ProductoId")]
        [DisplayName("ProductoId")]
        public int ProductoId { get; set; }

        [ForeignKey("PromocionId")]
        [DisplayName("PromocionId")]
        public int PromocionId { get; set; }

    }
}
