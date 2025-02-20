using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
    public class Login
    {
        [Display(Name = " Correo")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Correo { get; set; }

        [Display(Name = " Contraseña")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Contraseña { get; set; }
    }
}