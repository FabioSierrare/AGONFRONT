using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
    public class ProductoEnCarrito
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string UrlImagen { get; set; }
    }
}