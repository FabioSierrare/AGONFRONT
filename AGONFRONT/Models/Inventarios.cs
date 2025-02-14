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
    public class Inventarios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Producto")]
        public int ProductoId { get; set; }

        [Display(Name = " Cantidad de productos")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int Cantidad { get; set; }


        [Display(Name = " Tiempo de la ultima actualizacion")]
        [DataType(DataType.DateTime)]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime UltimaActualizacion { get; set; }
    }
}
