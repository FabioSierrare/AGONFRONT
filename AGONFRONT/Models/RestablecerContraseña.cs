using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
	public class RestablecerContraseña
	{
        public string Codigo { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        public string NuevaContraseña { get; set; }

        [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContraseña { get; set; }
    }
}