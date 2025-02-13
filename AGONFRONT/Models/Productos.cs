using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class Productos
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [Required]
        [DisplayName("Descripcion")]
        public string Descripcion { get; set; }

        [Required]
        [DisplayName("Precio")]
        public decimal Precio { get; set; }

        [Required]
        [DisplayName("Stock")]
        public int Stock { get; set; }

        [Required]
        [DisplayName("FechaCreacion")]
        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }


        [ForeignKey("CategoriaId")]
        [DisplayName("CategoriaId")]

        public int CategoriaId { get; set; }


        [ForeignKey("VendedorId")]
        [DisplayName("VendedorId")]

        public int VendedorId { get; set; }     

    }
}
