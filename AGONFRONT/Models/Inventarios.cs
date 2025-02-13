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
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("ProductoId")]
        [DisplayName("ProductoId")]
        public int ProductoId { get; set; }

        [DisplayName("Cantidad")]
        [Required(ErrorMessage = "El campo {0} es requerido")]

        public int Cantidad { get; set; }


        [DisplayName("UltimaActualizacion")]
        [DataType(DataType.DateTime)]
        [Required(ErrorMessage = "El campo {0} es requerido")]

        public DateTime UltimaActualizacion { get; set; }
    }
}
