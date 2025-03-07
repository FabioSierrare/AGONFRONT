using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using static AGONFRONT.Controllers.HomeController;

namespace AGONFRONT.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: X
        public ActionResult Register()
        {

            return View();
        }

        public ActionResult RegistroCliente()
        {
            return View();
        }
        public ActionResult RegistroVendedor()
        {
            return View();
        }



        // GET: X/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Usuarios model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Usuarios/PostUsuarios", content);

                    // Verificar el código de estado de la respuesta
                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        // Si es exitosa, puedes procesar la respuesta aquí
                    }
                    else
                    {
                        // Obtener más detalles sobre el código de estado y el contenido de la respuesta
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Si algo sale mal en el código, capturar el error
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }
        }

        public async Task<ActionResult> Misproductosvendedor()
        {
            List<Productos> productos = new List<Productos>();

            // Verificar si el token está en las cookies o en la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            string token = tokenCookie?.Value ?? tokenSession;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de envíos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // Obtener el ID del usuario desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Filtrar envíos que pertenecen al usuario autenticado
                    productos = productos.Where(p => p.VendedorId == int.Parse(userId)).ToList();

                    if (!productos.Any())
                    {
                        TempData["Error"] = "No se encontraron envíos para el usuario logueado.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(productos);
        }

        // Método para obtener el ID del usuario desde el token JWT
        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }

        public async Task<ActionResult> UpdatePerfilVendedor()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            // Verificar si el token está en las cookies
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            // Depuración para ver si las cookies están presentes
            Console.WriteLine("BearerToken Cookie en Request: " + tokenCookie?.Value);
            Console.WriteLine("TokenExpirationTime Cookie en Request: " + tokenExpirationCookie?.Value);

            // Si el token está ausente, redirigir al login
            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home"); // Redirige al login
            }

            // Si el token está presente en las cookies, continuar
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar la fecha de expiración de la cookie
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            // Si la fecha de expiración ha pasado, eliminar la cookie y redirigir
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

                HttpResponseMessage response = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los datos del usuario.";
                }
            }

            return View(usuarios);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUsuario(Usuarios usuario)
        {
            // Obtener el token de la cookie o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            // Corregir la asignación del token
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar si el token está vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync($"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario actualizado correctamente.";
                }
                else
                {
                    // Leer el contenido de la respuesta para ver los detalles del error
                    string errorMessage = await response.Content.ReadAsStringAsync();

                    // Guardar el mensaje detallado en TempData para mostrarlo en la vista
                    TempData["Error"] = $"Error al actualizar usuario: {errorMessage}";

                    // También puedes imprimirlo en la consola para depuración
                    Console.WriteLine($"Error en API: {errorMessage}");

                    // Regresar a la vista sin redirigir para mostrar los errores
                    return View("UpdatePerfilVendedor");
                }
            }

            return View("UpdatePerfilVendedor");
        }

        public async Task<ActionResult> GestionarProductos()
        {
            List<Productos> productos = new List<Productos>();

            var tokenCookie = Request.Cookies["BearerToken"]?.Value; // ✅ Extraer el valor del cookie
            var tokenSession = Session["BearerToken"] as string;

            string token = tokenCookie ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de envíos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // ✅ Obtener el ID del usuario desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    ViewBag.UsuarioId = userId; // ✅ Pasar ID del usuario a la vista

                    // Filtrar productos del usuario autenticado
                    productos = productos.Where(p => p.VendedorId == int.Parse(userId)).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(productos);
        }

        private async Task<List<Categoria>> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Categorias/GetCategorias");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(res);
                }
            }
            return categorias;
        }
        public async Task<ActionResult> AgregarProducto(Productos model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Productos/PostProductos", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }
        }

        public async Task<ActionResult> EditarProducto(int id)
        {
            var producto = await ObtenerProductoPorId(id);
            return View(producto);
        }

        [HttpPost]
        public async Task<ActionResult> EditarProducto(Productos producto)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(producto), Encoding.UTF8, "application/json");
                var response = await client.PutAsync("api/Productos/ActualizarProducto", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto actualizado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el producto.";
                    return View(producto);
                }
            }
        }

        public async Task<ActionResult> EliminarProducto(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                var response = await client.DeleteAsync($"api/Productos/EliminarProducto/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto eliminado con éxito.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el producto.";
                }
            }
            return RedirectToAction("Index");
        }

        private async Task<List<Productos>> ObtenerProductos()
        {
            List<Productos> productos = new List<Productos>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
            }
            return productos;
        }

        private async Task<Productos> ObtenerProductoPorId(int id)
        {
            Productos producto = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    producto = JsonConvert.DeserializeObject<Productos>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudo obtener el producto.";
                }
            }
            return producto;
        }

    }
}
