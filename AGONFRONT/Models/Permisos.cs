using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class Permisos
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Nombre")]
        public string Nombre { get; set; }
    }
}
