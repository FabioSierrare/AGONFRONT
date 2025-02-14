using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace AGONFRONT.Models
{
    public class Comentarios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name ="Id del Usuario")]
        public int UsuarioId { get; set; }

        [Display(Name = "Id del Producto")]
        public int ProductoId { get; set; }

        [Display(Name = " Comentario")]
        [MaxLength(300)]
        public string ComentarioTexto { get; set; }

        [Display(Name = " Fecha del Comentario")]
        [DataType(DataType.DateTime)]
        public DateTime FechaComentario { get; set; }
    }
}
