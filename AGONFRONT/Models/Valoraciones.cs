﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
     public class Valoraciones
     {

        public int Id { get; set; }
        public int UsuarioId { get; set; }
  
        public int ProductoId { get; set; }
        public int Valor { get; set; } 
        public DateTime FechaValoracion { get; set; }
    }
}
