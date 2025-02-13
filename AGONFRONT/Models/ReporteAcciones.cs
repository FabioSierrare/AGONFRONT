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
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("UsuarioId")]
        [DisplayName("UsuarioId")]
        public int UsuarioId { get; set; }

        [DisplayName("Descripcion")]
        [Required]
        public string Descripcion { get; set; }

        [DisplayName("FechaReporte")]
        [DataType(DataType.DateTime)]
        public DateTime FechaReporte { get; set; }
    }
}
