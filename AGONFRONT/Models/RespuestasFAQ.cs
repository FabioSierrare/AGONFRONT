using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class RespuestasFAQ
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Pregunta")]
        [Required]
        public string Pregunta { get; set; }

        [DisplayName("Respuesta")]
        [Required]
        public string Respuesta { get; set; }
    }
}
