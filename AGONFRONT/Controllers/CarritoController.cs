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
        /// <summary>
        /// Muestra la vista del carrito con la lista de productos almacenada en la sesión.
        /// </summary>
        /// <returns>Vista con la lista de <see cref="ProductoEnCarrito"/>.</returns>
        public async Task<ActionResult> Carrito()
        {
            List<ProductoEnCarrito> listCarrito = new List<ProductoEnCarrito>();
            if (Session["Carrito"] != null)
            {
                listCarrito = Session["Carrito"] as List<ProductoEnCarrito>;
            }
            return View(listCarrito);
        }

        /// <summary>
        /// Agrega un producto al carrito. Si ya existe, incrementa su cantidad.
        /// </summary>
        /// <param name="producto">Objeto <see cref="ProductoEnCarrito"/> con la información del producto a agregar.</param>
        /// <returns>Redirige a la acción <c>Carrito</c>.</returns>
        [HttpPost]
        public async Task<ActionResult> Agregar(ProductoEnCarrito producto)
        {
            List<ProductoEnCarrito> listCarrito = Session["Carrito"] as List<ProductoEnCarrito>;

            if (listCarrito == null)
            {
                listCarrito = new List<ProductoEnCarrito>();
            }

            var productoExistente = listCarrito.FirstOrDefault(p => p.ProductoId == producto.ProductoId);
            if (productoExistente != null)
            {
                productoExistente.Cantidad++;
            }
            else
            {
                listCarrito.Add(producto);
            }

            Session["Carrito"] = listCarrito;

            return RedirectToAction("Carrito");
        }

        /// <summary>
        /// Elimina un producto del carrito según su ID.
        /// </summary>
        /// <param name="id">ID del producto a eliminar.</param>
        /// <returns>Redirige a la acción <c>Carrito</c>.</returns>
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

        /// <summary>
        /// Actualiza la cantidad de un producto en el carrito.
        /// </summary>
        /// <param name="id">ID del producto a actualizar.</param>
        /// <param name="cantidad">Nueva cantidad del producto.</param>
        /// <returns>Redirige a la acción <c>Carrito</c>.</returns>
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

        /// <summary>
        /// Obtiene la lista del carrito desde la sesión de forma asincrónica.
        /// </summary>
        /// <returns>Una tarea que representa la operación y contiene una lista de <see cref="ProductoEnCarrito"/>.</returns>
        private Task<List<ProductoEnCarrito>> ObtenerCarritoAsync()
        {
            var carrito = Session["Carrito"] as List<ProductoEnCarrito>;
            return Task.FromResult(carrito ?? new List<ProductoEnCarrito>());
        }

        /// <summary>
        /// Guarda la lista del carrito en la sesión.
        /// </summary>
        /// <param name="carrito">Lista de <see cref="ProductoEnCarrito"/> a guardar en la sesión.</param>
        private void GuardarCarrito(List<ProductoEnCarrito> carrito)
        {
            Session["Carrito"] = carrito;
        }

    }
}