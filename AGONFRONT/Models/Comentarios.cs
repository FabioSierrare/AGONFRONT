using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
    public class Comentarios
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("UsuarioId")]
        [DisplayName("UsuarioId")]
        public int UsuarioId { get; set; }

        [DisplayName("ProductoId")]
        [ForeignKey("ProductoId")]
        public int ProductoId { get; set; }

        [DisplayName("ComentarioTexto")]
        [MaxLength(300)]
        public string ComentarioTexto { get; set; }

        [DisplayName("FechaComentario")]
        [DataType(DataType.DateTime)]
        public DateTime FechaComentario { get; set; }
    }
}
