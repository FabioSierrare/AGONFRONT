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
        /// <summary>
        /// Acción asincrónica que obtiene y muestra la lista de envíos filtrados para el usuario autenticado.
        /// Verifica la existencia y validez del token JWT almacenado en cookies o sesión antes de hacer la petición a la API.
        /// En caso de error, redirige al usuario a la página de inicio de sesión mostrando un mensaje de error.
        /// </summary>
        /// <returns>
        /// Retorna la vista con el modelo <see cref="EnviosViewModel"/> que contiene la lista de envíos.
        /// En caso de error o sesión inválida, redirige a la acción "Iniciar" del controlador "Home".
        /// </returns>
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
        /// <summary>
        /// Procesa el registro de un nuevo envío verificando la validez del modelo, 
        /// la existencia del pedido, y que no se haya registrado previamente.
        /// </summary>
        /// <param name="model">Modelo que contiene los datos del envío a registrar.</param>
        /// <returns>Una acción de redirección hacia la vista correspondiente según el resultado.</returns>
        public async Task<ActionResult> EnviosPost(Models.EnviosViewModel model)
        {
            // Validación del modelo recibido
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

            // Obtención del token de autenticación desde cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // Validación opcional de la fecha de expiración del token
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Clear();

                // Llamadas a las API para obtener listas de pedidos, usuarios y envíos
                var respPedidos = await client.GetAsync("api/Pedidos/GetPedidos");
                var respUsuarios = await client.GetAsync("api/Usuarios/GetUsuarios");
                var respEnvios = await client.GetAsync("api/Envios/GetEnvios");

                // Verificar que todas las respuestas hayan sido exitosas
                if (!respPedidos.IsSuccessStatusCode || !respUsuarios.IsSuccessStatusCode || !respEnvios.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Error al obtener datos (Pedidos: {respPedidos.StatusCode}, Usuarios: {respUsuarios.StatusCode}, Envios: {respEnvios.StatusCode}).";
                    return RedirectToAction("Iniciar", "Home");
                }

                // Deserialización de las respuestas en listas
                var pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(await respPedidos.Content.ReadAsStringAsync()) ?? new List<Pedidos>();
                var usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(await respUsuarios.Content.ReadAsStringAsync()) ?? new List<Usuarios>();
                var envios = JsonConvert.DeserializeObject<List<Envios>>(await respEnvios.Content.ReadAsStringAsync()) ?? new List<Envios>();

                // Buscar el pedido correspondiente al ID ingresado
                var pedidoSeleccionado = pedidos.FirstOrDefault(p => p.Id == model.Envios.PedidoId);
                if (pedidoSeleccionado == null)
                {
                    TempData["Error"] = "No se encontró el pedido.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }

                // Obtener el cliente asociado al pedido
                var cliente = usuarios.FirstOrDefault(u => u.Id == pedidoSeleccionado.ClienteId);
                if (cliente == null)
                {
                    TempData["Error"] = "No se encontró el cliente asociado al pedido.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }

                // Verificar si ya existe un envío registrado para este pedido
                bool yaExiste = envios.Any(e => e.PedidoId == model.Envios.PedidoId);
                if (yaExiste)
                {
                    TempData["Error"] = "Error al enviar los datos, verifique que no esté agregando un pedido ya existente.";
                    return RedirectToAction("GestionEnvios", "Envios");
                }

                // Asignar dirección del cliente como ubicación del envío
                model.Envios.Ubicacion = cliente.Direccion;

                // Enviar los datos del nuevo envío a la API
                string json = JsonConvert.SerializeObject(model.Envios);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Envios/PostEnvios", content);

                // Verificar si la operación fue exitosa
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
        // Método para obtener el ID del usuario desde el token JWT
        /// <summary>
        /// Extrae el ID del usuario autenticado desde el token JWT.
        /// </summary>
        /// <param name="token">Token JWT del cual se extraerá el ID del usuario.</param>
        /// <returns>El ID del usuario si se encuentra en el token, o null si no existe.</returns>
        public string GetLoggedInUserId(string token)
        {
            // 🛡️ Crear una instancia para manejar el token JWT
            var handler = new JwtSecurityTokenHandler();

            // 📦 Leer y deserializar el token JWT
            var jwtToken = handler.ReadJwtToken(token);

            // 🔍 Buscar el claim que contiene el ID del usuario (por convención es NameIdentifier)
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            // 📤 Devolver el valor del claim (ID del usuario) o null si no se encontró
            return userIdClaim?.Value;
        }
    }
}
