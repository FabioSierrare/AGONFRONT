using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
    public class Productos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "El nombre solo puede contener letras, números y espacios.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción del producto")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Precio del producto")]
        [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor que cero.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Formato de precio inválido. Use hasta dos decimales.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Stock del producto")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número entero positivo.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha de creación")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Id de la Categoría")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un vendedor.")]
        [Display(Name = "Id del Vendedor")]
        public int VendedorId { get; set; }

        [Display(Name = "Url de la imagen")]
        public string UrlImagen { get; set; }

        [NotMapped]
        public Categoria Categoria { get; set; }
    }
}
