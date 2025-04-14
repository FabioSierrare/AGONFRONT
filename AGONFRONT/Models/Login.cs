using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGONFRONT.Models
{
    public class Login
    {
        [JsonProperty("correo")]
        [Display(Name = "Correo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Correo { get; set; }

        [JsonProperty("contraseña")]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Contraseña { get; set; }
    }
}
