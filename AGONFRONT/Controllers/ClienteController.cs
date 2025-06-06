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
using AGONFRONT.Filters;
using AGONFRONT.Utils;

namespace AGONFRONT.Controllers
{
    //Para toda la parte de cliente:

    public class ClienteController : Controller
    {
        public ActionResult RespuestasFAQ()
        {

            return View();
        }


        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

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


        /// <summary>
        /// Carga los datos del perfil del cliente, incluyendo la lista de usuarios y pedidos,
        /// obtenidos desde los servicios API correspondientes.
        /// </summary>
        /// <returns>
        /// Una vista con la lista de usuarios como modelo y los pedidos almacenados en el ViewBag.
        /// </returns>
        public async Task<ActionResult> EditarPerfilCliente()
        {
            List<Usuarios> usuarios = new List<Usuarios>();
            List<Pedidos> pedidos = new List<Pedidos>();

            // Obtener token de autenticación desde cookie o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Cargar usuarios desde el servicio API
                var responseUsuarios = await client.GetAsync("api/Usuarios/GetUsuarios");
                if (responseUsuarios.IsSuccessStatusCode)
                {
                    var resUsuarios = await responseUsuarios.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(resUsuarios);
                }

                // Cargar pedidos desde el servicio API
                var responsePedidos = await client.GetAsync("api/Pedidos/GetPedidos");
                if (responsePedidos.IsSuccessStatusCode)
                {
                    var resPedidos = await responsePedidos.Content.ReadAsStringAsync();
                    pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(resPedidos);
                }
            }

            // Se pasan los pedidos mediante ViewBag y los usuarios como modelo de la vista
            ViewBag.Pedidos = pedidos;
            return View(usuarios);
        }



        [HttpPost]
        /// <summary>
        /// Actualiza la información de un cliente utilizando una solicitud PUT hacia la API de usuarios.
        /// </summary>
        /// <param name="usuario">Objeto <see cref="Usuarios"/> con los datos actualizados del cliente.</param>
        /// <returns>
        /// Redirige a la acción <c>EditarPerfilCliente</c> si la actualización es exitosa,
        /// de lo contrario redirige a la misma vista con un mensaje de error.
        /// </returns>
        public async Task<ActionResult> UpdateCliente(Usuarios usuario)
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            // Obtener el token desde la cookie o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Serializar el usuario y enviarlo como contenido JSON
                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                // Enviar solicitud PUT a la API para actualizar el usuario
                HttpResponseMessage response = await client.PutAsync($"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario actualizado correctamente.";

                    // Actualiza la cookie con el nuevo correo del usuario
                    Response.Cookies["UserEmail"].Value = usuario.Correo;

                    return RedirectToAction("EditarPerfilCliente", "Cliente");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar usuario: {errorMessage}";
                    Console.WriteLine($"Error en API: {errorMessage}");

                    return RedirectToAction("EditarPerfilCliente", "Cliente");
                }
            }
        }


        [HttpPost]
        /// <summary>
        /// Cambia la contraseña de un usuario en el sistema.
        /// </summary>
        /// <param name="usuario">
        /// Objeto <see cref="Usuarios"/> que contiene el ID del usuario y la nueva contraseña (sin encriptar).
        /// </param>
        /// <returns>
        /// Devuelve una acción que redirige al usuario a la vista de inicio si la actualización fue exitosa.
        /// En caso de error, muestra nuevamente la vista de edición del perfil con un mensaje de error.
        /// </returns>
        public async Task<ActionResult> CambiarContraseña(Usuarios usuario)
        {
            // Obtener el token desde cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // Si no hay token, redirigir al inicio de sesión
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Encriptar la nueva contraseña antes de enviarla a la API
            usuario.Contraseña = Encriptador.Encriptar(usuario.Contraseña);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Convertir el objeto usuario a JSON
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                // Enviar la solicitud PUT a la API para actualizar la contraseña
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Contraseña actualizada correctamente.";
                }
                else
                {
                    // Si hubo error, mostrar mensaje y mantener la vista actual
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar la contraseña: {errorMessage}";
                    Console.WriteLine($"Error en API: {errorMessage}");

                    return View("EditarPerfilCliente");
                }
            }

            // Redirigir al inicio si todo fue exitoso
            return RedirectToAction("Iniciar", "Home");
        }


        [HttpPost]
        /// <summary>
        /// Agrega una nueva entrada de Pregunta Frecuente (FAQ) al sistema mediante una llamada a la API.
        /// </summary>
        /// <param name="model">
        /// Objeto <see cref="RespuestasFAQ"/> que contiene la pregunta y la respuesta que se desean registrar.
        /// </param>
        /// <returns>
        /// Devuelve una acción que redirige a la vista <c>RespuestasFAQ</c> con un mensaje de éxito o error.
        /// </returns>
        public async Task<ActionResult> AgregarRespuestasFAQ(RespuestasFAQ model)
        {
            try
            {
                // Validar si la pregunta está vacía
                if (string.IsNullOrEmpty(model.Pregunta))
                {
                    TempData["Error"] = "Por favor, completa la pregunta.";
                    return RedirectToAction("RespuestasFAQ");
                }

                // Si la respuesta está vacía, asignar null
                if (string.IsNullOrEmpty(model.Respuesta))
                {
                    model.Respuesta = null;
                }

                // Preparar y enviar la solicitud HTTP POST a la API
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/RespuestasFAQ/PostRespuestaFAQ", content);

                    // Manejar errores si la respuesta no fue exitosa
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("RespuestasFAQ");
                    }
                }

                // Mostrar mensaje de éxito
                TempData["Success"] = "Pregunta frecuente agregada correctamente.";
                return RedirectToAction("RespuestasFAQ");
            }
            catch (Exception ex)
            {
                // Manejar errores no controlados
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("RespuestasFAQ");
            }
        }

    }
}
