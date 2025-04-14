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
using System.Globalization;

namespace AGONFRONT.Controllers
{
    public class ClienteController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        // Método para obtener el ID del usuario desde el token JWT
        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }

        public async Task<ActionResult> EditarPerfilCliente()
        {
            List<Usuarios> usuarios = new List<Usuarios>();
            List<Pedidos> pedidos = new List<Pedidos>();

            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            string token = tokenCookie?.Value ?? tokenSession;

            if (tokenExpirationCookie != null &&
                DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) &&
                DateTime.Now > expiryDate)
            {
                Response.Cookies.Remove("BearerToken");
                Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Obtener usuarios
                var responseUsuarios = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (responseUsuarios.IsSuccessStatusCode)
                {
                    var resUsuarios = await responseUsuarios.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(resUsuarios);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los datos del usuario.";
                }

                // Obtener pedidos
                var responsePedidos = await client.GetAsync("api/Pedidos/GetPedidos");

                if (responsePedidos.IsSuccessStatusCode)
                {
                    var resPedidos = await responsePedidos.Content.ReadAsStringAsync();
                    var todosLosPedidos = JsonConvert.DeserializeObject<List<Pedidos>>(resPedidos);

                    // Obtener ID del usuario desde el token
                }
            }

            ViewBag.Pedidos = pedidos;
            return View(usuarios);
        }


        [HttpPost]
        public async Task<ActionResult> UpdateCliente(Usuarios usuario)
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
                    return View("EditarPerfilCliente");
                }
            }

            return View("EditarPerfilCliente");
        }

        public async Task<ActionResult> HistorialPedidos()
        {
            List<Pedidos> pedidos = new List<Pedidos>();

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

                HttpResponseMessage response = await client.GetAsync("api/Pedidos/GetPeddidos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los datos del usuario.";
                }
            }

            return View(pedidos);
        }

    }
}