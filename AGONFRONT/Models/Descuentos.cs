using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
    public class Descuentos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Tipo")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Tipo { get; set; }

        [Display(Name = " Nombre")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Nombre { get; set; }

        [Display(Name = " Codigo")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Codigo { get; set; }

        [Display(Name = " Descuento")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public decimal Descuento { get; set; }

        [Display(Name = " Fecha de inicio del descuento")]
        [DataType(DataType.DateTime)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha final del descuento")]
        [DataType(DataType.DateTime)]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = " El campo {0} es requerido")]
        public int VendedorId { get; set; } 
    }
}
