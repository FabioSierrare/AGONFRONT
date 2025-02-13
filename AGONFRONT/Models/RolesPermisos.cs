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
    public class RolesPermisos
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [ForeignKey("RolId")]
        [DisplayName("RolId")]
        public int RolId { get; set; }

        [ForeignKey("PermisoId")]
        [DisplayName("PermisoId")]
        public int PermisoId { get; set; }
    }
}
