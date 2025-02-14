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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Producto")]
        public int ProductoId { get; set; }

        [Display(Name = " Id de la Promocion")]
        public int PromocionId { get; set; }

    }
}
