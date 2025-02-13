using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class Notificaciones
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Titulo")]
        [Required]
        public string Titulo { get; set; }

        [DisplayName("Mensaje")]
        [Required]

        public string Mensaje { get; set; }

        [DisplayName("FechaEnvio")]
        [DataType(DataType.Date)]
        public DateTime FechaEnvio { get; set; }

    }
}
