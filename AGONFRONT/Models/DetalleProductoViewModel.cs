using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
    public class DetalleProductoViewModel
    {
        public Productos Producto { get; set; }
        public ProductoEnCarrito ProductoCarrito { get; set; }
    }
}