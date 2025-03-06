using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace AGONFRONT.Models
{
    public class GestionarProductos
    {
        public List<Productos> Productos { get; set; } = new List<Productos>();
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
        public Productos Producto { get; set; } = new Productos();
    }
}