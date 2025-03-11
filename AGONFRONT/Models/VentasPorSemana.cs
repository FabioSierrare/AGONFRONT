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
    public class VentasPorSemana
    {
        public int semana {  get; set; }
        public int año { get; set; }
        public decimal totalVentas { get; set; }
        public int productosVendidos { get; set; }
    }
}
