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

        /// <summary>
        /// Obtiene la lista de productos desde la API y los muestra en la vista.
        /// Además, valida la sesión del usuario verificando el token y su expiración.
        /// </summary>
        /// <returns>
        /// Vista con la lista de productos si la sesión es válida.
        /// Redirige a la página de inicio de sesión si la sesión ha expirado.
        /// </returns>
        public async Task<ActionResult> Productos()
        {
            List<Productos> productos = new List<Productos>();

            // Obtener el token de autenticación desde cookie o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar si la cookie de expiración de token existe y parsearla
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            // Validar si el token ha expirado
            if (expirationTime.HasValue && DateTime.Now > expirationTime.Value)
            {
                // Eliminar cookies de autenticación porque la sesión expiró
                HttpContext.Response.Cookies.Remove("BearerToken");
                HttpContext.Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                // Solicitar la lista de productos a la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    // Deserializar la respuesta JSON a una lista de productos
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Obtener los IDs de productos y almacenarlos en una cookie para uso posterior
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


        /// <summary>
        /// Obtiene los detalles de un producto específico junto con sus comentarios.
        /// También obtiene el ID del usuario basado en el correo almacenado en cookie.
        /// </summary>
        /// <param name="id">El ID del producto a mostrar.</param>
        /// <returns>
        /// Vista con los detalles del producto, comentarios y datos del usuario.
        /// En caso de error, redirige a la lista de productos con un mensaje de error.
        /// </returns>
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

                    // Obtener producto por ID desde la API
                    HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        producto = JsonConvert.DeserializeObject<Productos>(res);

                        // Preparar ViewModel con producto y datos para carrito
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

                    // Obtener todos los comentarios y filtrar por producto
                    HttpResponseMessage comentariosResponse = await client.GetAsync("api/Comentarios/GetComentarios");
                    if (comentariosResponse.IsSuccessStatusCode)
                    {
                        var comentariosJson = await comentariosResponse.Content.ReadAsStringAsync();
                        var comentarios = JsonConvert.DeserializeObject<List<Comentarios>>(comentariosJson);
                        ViewBag.Comentarios = comentarios.Where(c => c.ProductoId == id).ToList();
                    }

                    // Obtener email del usuario desde cookie para buscar su ID
                    string emailUsuario = Request.Cookies["UserEmail"]?.Value;
                    if (!string.IsNullOrEmpty(emailUsuario))
                    {
                        HttpResponseMessage usuariosResponse = await client.GetAsync("api/Usuarios/GetUsuarios");
                        if (usuariosResponse.IsSuccessStatusCode)
                        {
                            var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                            var usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(usuariosJson);

                            // Buscar usuario con correo igual al email en cookie
                            var usuario = usuarios.FirstOrDefault(u => u.Correo == emailUsuario);
                            if (usuario != null)
                            {
                                ViewBag.UserId = usuario.Id;

                                // Guardar UserId en cookie para futuras peticiones
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
        /// <summary>
        /// Permite a un usuario autenticado agregar un comentario a un producto específico.
        /// </summary>
        /// <param name="productoId">El ID del producto al que se agregará el comentario.</param>
        /// <param name="comentarioTexto">El texto del comentario que el usuario desea publicar.</param>
        /// <returns>
        /// Redirige a la vista de detalles del producto.
        /// En caso de no estar autenticado o error, redirige a la página de inicio de sesión con un mensaje.
        /// </returns>
        public async Task<ActionResult> Comentar(int productoId, string comentarioTexto)
        {
            // Obtener correo del usuario desde la cookie
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

                // Obtener lista de usuarios desde la API
                HttpResponseMessage usuariosResponse = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (usuariosResponse.IsSuccessStatusCode)
                {
                    var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                    var usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(usuariosJson);

                    // Buscar usuario cuyo correo coincida con el email de la cookie
                    var usuario = usuarios.FirstOrDefault(u => u.Correo == emailUsuario);
                    if (usuario != null)
                        userId = usuario.Id;
                }

                // Validar que se haya encontrado un usuario válido
                if (userId == null)
                {
                    TempData["Error"] = "No se pudo validar tu identidad.";
                    return RedirectToAction("Iniciar", "Home");
                }

                // Crear objeto comentario con la información proporcionada
                var nuevoComentario = new Comentarios
                {
                    UsuarioId = userId.Value,
                    ProductoId = productoId,
                    ComentarioTexto = comentarioTexto,
                    FechaComentario = DateTime.Now
                };

                // Serializar comentario y enviarlo a la API para guardarlo
                var json = JsonConvert.SerializeObject(nuevoComentario);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Comentarios/PostComentarios", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudo registrar el comentario.";
                }
            }

            // Redirigir a los detalles del producto para mostrar el comentario agregado
            return RedirectToAction("Detalles", new { id = productoId });
        }

        // Puedes mantener los demás métodos: Frutas, Verduras, etc. igual que estaban antes
        /// <summary>
        /// Obtiene la lista de productos de tipo cereal desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos tipo cereal.</returns>
        public async Task<ActionResult> Cereales()
        {
            // Lista para almacenar los productos tipo cereal
            List<Productos> cereal = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base para las solicitudes HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Solicita la lista completa de productos desde la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido JSON de la respuesta
                    var res = await response.Content.ReadAsStringAsync();

                    // Convierte el JSON en una lista de objetos Productos
                    cereal = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    // Guarda un mensaje de error para mostrar en la vista
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Retorna la vista pasando la lista de productos cereales
            return View(cereal);
        }

        /// <summary>
        /// Obtiene la lista de productos de tipo fruta desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos tipo fruta.</returns>
        public async Task<ActionResult> Frutas()
        {
            // Lista para almacenar los productos tipo fruta
            List<Productos> frutas = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base para las solicitudes HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Solicita la lista completa de productos desde la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido JSON de la respuesta
                    var res = await response.Content.ReadAsStringAsync();

                    // Convierte el JSON en una lista de objetos Productos
                    frutas = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    // Guarda un mensaje de error para mostrar en la vista
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Retorna la vista pasando la lista de productos frutas
            return View(frutas);
        }

        /// <summary>
        /// Obtiene la lista de productos de tipo verdura desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos tipo verdura.</returns>
        public async Task<ActionResult> Verduras()
        {
            // Lista para almacenar los productos tipo verdura
            List<Productos> verduras = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base para las solicitudes HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Solicita la lista completa de productos desde la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido JSON de la respuesta
                    var res = await response.Content.ReadAsStringAsync();

                    // Convierte el JSON en una lista de objetos Productos
                    verduras = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    // Guarda un mensaje de error para mostrar en la vista
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Retorna la vista pasando la lista de productos verduras
            return View(verduras);
        }


        /// <summary>
        /// Obtiene la lista de productos de tipo granja desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos tipo granja.</returns>
        public async Task<ActionResult> Granja()
        {
            // Lista para almacenar los productos tipo granja
            List<Productos> granja = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base para las solicitudes HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Solicita la lista completa de productos desde la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido JSON de la respuesta
                    var res = await response.Content.ReadAsStringAsync();

                    // Convierte el JSON en una lista de objetos Productos
                    granja = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    // Guarda un mensaje de error para mostrar en la vista
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Retorna la vista pasando la lista de productos granja
            return View(granja);
        }


        /// <summary>
        /// Obtiene la lista de productos de tipo tubérculos desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos tipo tubérculos.</returns>
        public async Task<ActionResult> Tuberculos()
        {
            // Lista para almacenar los productos tipo tubérculos
            List<Productos> tuberculos = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base para las solicitudes HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Solicita la lista completa de productos desde la API
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido JSON de la respuesta
                    var res = await response.Content.ReadAsStringAsync();

                    // Convierte el JSON en una lista de objetos Productos
                    tuberculos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    // Guarda un mensaje de error para mostrar en la vista
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            // Retorna la vista pasando la lista de productos tubérculos
            return View(tuberculos);
        }

    }
}
