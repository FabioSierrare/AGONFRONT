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
    public class ImagenProducto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = " Dirección de la imagen")]
        [Url]
        public string UrlImagen { get; set; }


        [Display(Name = " Id del Producto")]
        public int ProductoId { get; set; }


    }
}
