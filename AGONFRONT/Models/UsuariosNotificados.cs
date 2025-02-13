﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class UsuariosNotificados
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int NotificacionId { get; set; }
        public bool Leido { get; set; } 
        public DateTime? FechaLeido { get; set; }
    }
}
