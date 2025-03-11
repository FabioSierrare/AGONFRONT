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
    public class Estadisticasx
    {
        public List<VentasPorSemana> VentasPorSemana { get; set; }
        public List<ProductosMasVendidos> ProductosMasVendidos { get; set; }
    }
}
