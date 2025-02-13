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
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("UrlImagen")]
        [Url]
        public string UrlImagen { get; set; }


        [DisplayName("Productoid")]
        [ForeignKey("ProductoId")]
        public int ProductoId { get; set; }


    }
}
