using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class LogsSistema
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nivel")]
        public string Nivel { get; set; }

        [DisplayName("Mensaje")]
        public string Mensaje { get; set; }

        [DisplayName("FechaLog")]
        [DataType(DataType.Date)]
        public DateTime FechaLog { get; set; }
    }
}
