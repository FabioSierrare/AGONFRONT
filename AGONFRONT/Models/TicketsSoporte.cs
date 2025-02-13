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
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("UsuarioId")]
        [ForeignKey("UsuarioId")]
        public int UsuarioId { get; set; }

        [DisplayName("Titulo")]
        [Required]
        public string Titulo { get; set; }

        [DisplayName("Descripcion")]
        [Required]
        public string Descripcion { get; set; } 

        [DisplayName("Estado")]
        public string Estado { get; set; }

        [DisplayName("FechaCreacion")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }
    }
}
