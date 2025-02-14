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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del Envio")]
        public int EnvioId { get; set; }

        [Display(Name = " Envio")]
        public string Estado { get; set; }

        [Display(Name = " Ubicacion del envio")]
        public string Ubicacion { get; set; }

        [Display(Name = " Fecha del envio")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; }
    }
}
