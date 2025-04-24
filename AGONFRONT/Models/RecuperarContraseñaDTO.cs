using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
	public class RecuperarContraseñaDTO
	{
        [Display(Name = " Correo")]
        [Required(ErrorMessage = " El campo {0} es requerido")]
        public string Correo { get; set; }
    }
}