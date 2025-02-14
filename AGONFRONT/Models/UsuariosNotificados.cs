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
    public class UsuariosNotificados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Id del usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = " Id de la notificacion")]
        public int NotificacionId { get; set; }

        [Display(Name = " Usuario notificado")]
        public bool Leido { get; set; }

        [Display(Name = " Fecha vista por el usuario")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaLeido { get; set; }
    }
}
