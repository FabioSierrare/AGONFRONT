using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;

namespace AGONFRONT.Controllers
{
    public class CarritoController : Controller
    {
        // Mostrar el carrito
        public async Task<ActionResult> Carrito()
        {
            List<ProductoEnCarrito> listCarrito = new List<ProductoEnCarrito>();
            if (Session["Carrito"] != null)
            {
                listCarrito = Session["Carrito"] as List<ProductoEnCarrito>;
            }
            return View(listCarrito);
        }

        // Agregar un producto al carrito
        [HttpPost]
        public async Task<ActionResult> Agregar(ProductoEnCarrito producto)
        {
            List<ProductoEnCarrito> listCarrito = Session["Carrito"] as List<ProductoEnCarrito>;

            // Si no existe, crea un nuevo carrito
            if (listCarrito == null)
            {
                listCarrito = new List<ProductoEnCarrito>();
            }

            // Aquí puedes comprobar si el producto ya existe en el carrito. Si existe, aumenta la cantidad.
            var productoExistente = listCarrito.FirstOrDefault(p => p.ProductoId == producto.ProductoId);
            if (productoExistente != null)
            {
                productoExistente.Cantidad++;  // Si el producto ya está en el carrito, incrementa la cantidad
            }
            else
            {
                // Si el producto no está en el carrito, lo agregamos
                listCarrito.Add(producto);
            }

            // Guarda el carrito actualizado en la sesión
            Session["Carrito"] = listCarrito;

            // Redirige al carrito
            return RedirectToAction("Carrito");


            //var carrito = await ObtenerCarritoAsync();

            //var producto = carrito.FirstOrDefault(p => p.ProductoId == id);
            //if (producto != null)
            //{
            //    producto.Cantidad++;
            //}
            //else
            //{
            //    carrito.Add(new ProductoEnCarrito
            //    {
            //        ProductoId = id,
            //        Nombre = nombre,
            //        Cantidad = 1,
            //        UrlImagen = imagenUrl
            //    });
            //}

            //GuardarCarrito(carrito);
            //return RedirectToAction("Carrito");
        }

        // Eliminar un producto del carrito
        [HttpPost]
        public async Task<ActionResult> Eliminar(int id)
        {
            var carrito = await ObtenerCarritoAsync();
            var producto = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (producto != null)
            {
                carrito.Remove(producto);
                GuardarCarrito(carrito);
            }

            return RedirectToAction("Carrito");
        }

        // Actualizar la cantidad de un producto
        [HttpPost]
        public async Task<ActionResult> ActualizarCantidad(int id, int cantidad)
        {
            var carrito = await ObtenerCarritoAsync();
            var producto = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (producto != null && cantidad > 0)
            {
                producto.Cantidad = cantidad;
                GuardarCarrito(carrito);
            }

            return RedirectToAction("Carrito");
        }

        // Obtener carrito desde Session (asincrónicamente)
        private Task<List<ProductoEnCarrito>> ObtenerCarritoAsync()
        {
            var carrito = Session["Carrito"] as List<ProductoEnCarrito>;
            return Task.FromResult(carrito ?? new List<ProductoEnCarrito>());
        }

        // Guardar carrito en Session
        private void GuardarCarrito(List<ProductoEnCarrito> carrito)
        {
            Session["Carrito"] = carrito;
        }
    }
}