using AGONFRONT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AGONFRONT.Controllers
{
    public class ProductosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public async Task<ActionResult> Productos()
        {
            List<Productos> productos = new List<Productos>();

            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            string token = tokenCookie?.Value ?? tokenSession;

            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            if (expirationTime.HasValue && DateTime.Now > expirationTime.Value)
            {
                HttpContext.Response.Cookies.Remove("BearerToken");
                HttpContext.Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            var productosIds = productos.Select(p => p.Id).ToList();
            var productosIdsString = string.Join(",", productosIds);

            HttpCookie productosCookie = new HttpCookie("productosIds", productosIdsString)
            {
                Expires = DateTime.Now.AddHours(1),
                HttpOnly = true
            };
            Response.Cookies.Add(productosCookie);

            return View(productos);
        }

        public async Task<ActionResult> Detalles(int id)
        {
            Productos producto = null;
            DetalleProductoViewModel viewModel = new DetalleProductoViewModel();
            ViewBag.Comentarios = new List<Comentarios>();
            ViewBag.UserId = null;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    // Obtener producto
                    HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        producto = JsonConvert.DeserializeObject<Productos>(res);

                        viewModel.Producto = producto;
                        viewModel.ProductoCarrito = new ProductoEnCarrito
                        {
                            ProductoId = producto.Id,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio,
                            Cantidad = 1
                        };
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo obtener el producto.";
                        return RedirectToAction("Productos");
                    }

                    // Obtener comentarios
                    HttpResponseMessage comentariosResponse = await client.GetAsync("api/Comentarios/GetComentarios");
                    if (comentariosResponse.IsSuccessStatusCode)
                    {
                        var comentariosJson = await comentariosResponse.Content.ReadAsStringAsync();
                        var comentarios = JsonConvert.DeserializeObject<List<Comentarios>>(comentariosJson);
                        ViewBag.Comentarios = comentarios.Where(c => c.ProductoId == id).ToList();
                    }

                    // Obtener ID desde cookie UserEmail
                    string emailUsuario = Request.Cookies["UserEmail"]?.Value;
                    if (!string.IsNullOrEmpty(emailUsuario))
                    {
                        HttpResponseMessage usuariosResponse = await client.GetAsync("api/Usuarios/GetUsuarios");
                        if (usuariosResponse.IsSuccessStatusCode)
                        {
                            var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                            var usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(usuariosJson);
                            var usuario = usuarios.FirstOrDefault(u => u.Correo == emailUsuario);

                            if (usuario != null)
                            {
                                ViewBag.UserId = usuario.Id;

                                Response.Cookies.Add(new HttpCookie("UserId", usuario.Id.ToString())
                                {
                                    HttpOnly = true,
                                    Expires = DateTime.Now.AddHours(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction("Productos");
            }

            if (producto == null)
            {
                TempData["Error"] = "El producto no fue encontrado.";
                return RedirectToAction("Productos");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Comentar(int productoId, string comentarioTexto)
        {
            string emailUsuario = Request.Cookies["UserEmail"]?.Value;
            if (string.IsNullOrEmpty(emailUsuario))
            {
                TempData["Error"] = "Debes iniciar sesión para comentar.";
                return RedirectToAction("Iniciar", "Home");
            }

            int? userId = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage usuariosResponse = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (usuariosResponse.IsSuccessStatusCode)
                {
                    var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                    var usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(usuariosJson);
                    var usuario = usuarios.FirstOrDefault(u => u.Correo == emailUsuario);
                    if (usuario != null)
                        userId = usuario.Id;
                }

                if (userId == null)
                {
                    TempData["Error"] = "No se pudo validar tu identidad.";
                    return RedirectToAction("Iniciar", "Home");
                }

                var nuevoComentario = new Comentarios
                {
                    UsuarioId = userId.Value,
                    ProductoId = productoId,
                    ComentarioTexto = comentarioTexto,
                    FechaComentario = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(nuevoComentario);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Comentarios/PostComentarios", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudo registrar el comentario.";
                }
            }

            return RedirectToAction("Detalles", new { id = productoId });
        }

        // Puedes mantener los demás métodos: Frutas, Verduras, etc. igual que estaban antes
    }
}
