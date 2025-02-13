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
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("UsuarioId")]
        [DisplayName("UsuarioId")]
        public int UsuarioId { get; set; }

        [ForeignKey("NotificacionId")]
        [DisplayName("NotificacionId")]
        public int NotificacionId { get; set; }

        [DisplayName("UsuarioId")]
        public bool Leido { get; set; }

        [DisplayName("UsuarioId")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaLeido { get; set; }
    }
}
