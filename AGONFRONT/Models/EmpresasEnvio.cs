using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class EmpresasEnvio
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; }

        [DisplayName("Contacto")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Contacto { get; set; }
    }
}
