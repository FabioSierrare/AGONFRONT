﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGONFRONT.Models
{
    public class ImagenProducto
    {
        public int Id { get; set; } 
        public string UrlImagen { get; set; } 

        public int ProductoId { get; set; }
    }
}
