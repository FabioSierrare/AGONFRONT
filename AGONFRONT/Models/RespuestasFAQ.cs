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
    public class RespuestasFAQ
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Pregunta")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Pregunta { get; set; }

        [Display(Name = " Respuesta")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Respuesta { get; set; }
    }
}
