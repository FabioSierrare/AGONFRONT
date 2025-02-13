﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class TrackingEnvio
    {
        public int Id { get; set; }
        public int EnvioId { get; set; }
        public string Estado { get; set; }
        public string Ubicacion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
