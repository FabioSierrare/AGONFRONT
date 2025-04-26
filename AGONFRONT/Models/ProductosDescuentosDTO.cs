using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AGONFRONT.Models
{
	public class ProductosDescuentosDTO
	{
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string UrlImagen { get; set; }
        public decimal PrecioOriginal { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal PrecioFinal => PrecioOriginal - (PrecioOriginal * PorcentajeDescuento / 100);
    }
}