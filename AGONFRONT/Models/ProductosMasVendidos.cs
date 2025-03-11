using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGONFRONT.Models
{
    public class ProductosMasVendidos
    {
        public string Producto { get; set; }
        public int totalVendido { get; set; }
    }
}
