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
    public class Usuarios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Nombre")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Nombre { get; set; }

        [Display(Name = " Correo")]
        [EmailAddress(ErrorMessage = " El email no tiene un formato válido")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Correo { get; set; }

        [Display(Name = " Contraseña")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Contraseña { get; set; }

        [Display(Name = " Telefono")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Telefono { get; set; }

        [Display(Name = " Direccion")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Direccion { get; set; }

        [Display(Name = " Tipo de usuario")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string TipoUsuario { get; set; }

        [Display(Name =  "Fehca de creacion")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }
    }
}
