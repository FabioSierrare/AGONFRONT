using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace AGONFRONT.Models
{
    public class Usuarios
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string Nombre { get; set; }

        [DisplayName("Correo")]
        [Required]
        public string Correo { get; set; }

        [DisplayName("Contraseña")]
        [Required]
        public string Contraseña { get; set; }

        [DisplayName("Telefono")]
        [Required]
        public string Telefono { get; set; }

        [DisplayName("Direccion")]
        [Required]
        public string Direccion { get; set; }

        [DisplayName("TipoUsuario")]
        [Required]
        public string TipoUsuario { get; set; }

        [DisplayName("FehcaCreacion")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }
    }
}
