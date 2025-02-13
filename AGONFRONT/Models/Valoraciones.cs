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
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("UsuarioId")]
        [DisplayName("UsuarioId")]
        public int UsuarioId { get; set; }

        [ForeignKey("ProductoId")]
        [DisplayName("ProductoId")]
        public int ProductoId { get; set; }

        [DisplayName("Valor")]
        [Required]
        public int Valor { get; set; }

        [DisplayName("FechaValoracion")]
        [DataType(DataType.DateTime)]
        public DateTime FechaValoracion { get; set; }
    }
}
