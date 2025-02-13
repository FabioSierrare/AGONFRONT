using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AGONFRONT.Models
{
    public class Categoria
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nombre")]
        [Required (ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; } 
    }
}
