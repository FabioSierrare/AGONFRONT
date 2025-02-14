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
    public class TicketsSoporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = " Titulo")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Titulo { get; set; }

        [Display(Name = " Descripcion del ticket")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Descripcion { get; set; } 

        [Display(Name = " Estado del ticket")]
        public string Estado { get; set; }

        [Display(Name = " Fecha de creacion del ticket")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }
    }
}
