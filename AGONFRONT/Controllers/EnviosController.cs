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
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AGONFRONT.Utils;
using System.Reflection;


namespace AGONFRONT.Controllers
{
    public class EnviosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: Envios

        // GET: Envios/Details/5
        public async Task<ActionResult> GestionEnvios()
        {
            var envios = new EnviosViewModel();

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

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Hacer la petición GET
                    HttpResponseMessage response = await client.GetAsync($"api/Envios/GetEnviosFiltrados/{userId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = $"Error al obtener envíos (Código: {response.StatusCode}).";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res); // Esto imprimirá la respuesta JSON en la consola
                    envios.Envio = JsonConvert.DeserializeObject<List<Envio>>(res) ?? new List<Envio>();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(envios);
        }

        [HttpPost]
        public async Task<ActionResult> EnviosPost(Models.EnviosViewModel model)
        {
            List<Pedidos> pedido = new List<Pedidos>();
            List<Usuarios> usuario = new List<Usuarios>();
            List<Envios> envi = new List<Envios>();

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        TempData["Error"] = $"Campo: {key} - Error: {error.ErrorMessage}";
                    }
                }
                return RedirectToAction("Iniciar", "Home");
            }

            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar la fecha de expiración de la cookie
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Clear();
                var userId = GetLoggedInUserId(token);

                HttpResponseMessage resp = await client.GetAsync("api/Pedidos/GetPedidos");
                HttpResponseMessage respu = await client.GetAsync("api/Usuarios/GetUsuarios");
                HttpResponseMessage re = await client.GetAsync($"api/Envios/GetEnvios");

                if (!resp.IsSuccessStatusCode || !respu.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Error al obtener datos (Pedidos: {resp.StatusCode}, Usuarios: {respu.StatusCode}).";
                    return RedirectToAction("Iniciar", "Home");
                }

                var rest = await resp.Content.ReadAsStringAsync();
                pedido = JsonConvert.DeserializeObject<List<Pedidos>>(rest) ?? new List<Pedidos>();

                var respue = await respu.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<List<Usuarios>>(respue) ?? new List<Usuarios>(); // ✅ aquí corregido

                var resc = await re.Content.ReadAsStringAsync();
                envi = JsonConvert.DeserializeObject<List<Envios>>(resc) ?? new List<Envios>();

                var pedidoSeleccionado = pedido.FirstOrDefault(p => p.Id == model.Envios.PedidoId);

                if (pedidoSeleccionado == null)
                {
                    TempData["Error"] = "No se encontró el pedido.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }

                var cliente = usuario.FirstOrDefault(u => u.Id == pedidoSeleccionado.ClienteId);

                if (cliente == null)
                {
                    TempData["Error"] = "No se encontró el cliente asociado al pedido.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }

                bool yaExiste = envi.Any(e => e.PedidoId.ToString() == model.Envios.PedidoId.ToString());

                if (yaExiste)
                {
                    // Mostrar mensaje al usuario
                    TempData["Error"] = "Error al enviar los datos, Verifique que no este agregando un pedido ya existente.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }
                else
                {
                    // Continuar con la creación del envío
                }

                model.Envios.Ubicacion = cliente.Direccion;

                client.DefaultRequestHeaders.Clear();

                string json = JsonConvert.SerializeObject(model.Envios);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/Envios/PostEnvios", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Envío registrado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Error al enviar los datos.";
                }

                return RedirectToAction("GestionEnvios", "Envios");
            }
        }


        // Método para obtener el ID del usuario desde el token JWT
        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }
    }
}
