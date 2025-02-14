using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
     public class Valoraciones
     {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = " Id del Producto")]
        public int ProductoId { get; set; }

        [Display(Name = " Que calificacion le da al producto")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public int Valor { get; set; }

        [Display(Name = " Fecha de la valoracion")]
        [DataType(DataType.DateTime)]
        public DateTime FechaValoracion { get; set; }
    }
}
