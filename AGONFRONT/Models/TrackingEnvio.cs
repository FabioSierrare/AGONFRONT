using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AGONFRONT.Models
{
    public class TrackingEnvio
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("EnvioId")]
        [ForeignKey("EnvioId")]
        public int EnvioId { get; set; }

        [DisplayName("EnvioId")]
        public string Estado { get; set; }

        [DisplayName("Ubicacion")]
        public string Ubicacion { get; set; }

        [DisplayName("Fecha")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; }
    }
}
