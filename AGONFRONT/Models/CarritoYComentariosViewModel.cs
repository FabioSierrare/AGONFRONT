using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
    public class CarritoYComentariosViewModel
    {
        public List<ProductoEnCarrito> Productos { get; set; }
        public List<Comentarios> Comentarios { get; set; }
    }
}