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
    public class ReporteAcciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = " Descripcion del reporte")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Descripcion { get; set; }

        [Display(Name = " Fecha del Reporte")]
        [DataType(DataType.DateTime)]
        public DateTime FechaReporte { get; set; }
    }
}
