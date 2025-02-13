using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AGONFRONT.Models
{
    public class Promociones
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string Nombre { get; set; }

        [DisplayName("Descuento")]
        [Required]
        public decimal Descuento { get; set; }

        [DisplayName("FechaInicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [DisplayName("FechaFin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }
    }
}
