namespace AGONFRONT.Models
{
    public class Busquedas
    {
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public decimal PrecioConDescuento { get; set; }
        public string Url
        
        { get; set; }
        public decimal? Descuento { get; set; }
    }
}
