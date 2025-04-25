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
    [AuthorizeByRole("Cliente")]

    public class ClienteController : Controller
    {
        public ActionResult RespuestasFAQ()
        {

            return View();
        }


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
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Cargar usuarios
                var responseUsuarios = await client.GetAsync("api/Usuarios/GetUsuarios");
                if (responseUsuarios.IsSuccessStatusCode)
                {
                    var resUsuarios = await responseUsuarios.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(resUsuarios);
                }

                // Cargar pedidos
                var responsePedidos = await client.GetAsync("api/Pedidos/GetPedidos");
                if (responsePedidos.IsSuccessStatusCode)
                {
                    var resPedidos = await responsePedidos.Content.ReadAsStringAsync();
                    pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(resPedidos);
                }
            }

            ViewBag.Pedidos = pedidos; // ⬅️ los pedidos en el ViewBag
            return View(usuarios);     // ⬅️ los usuarios en el Model
        }


        [HttpPost]
        public async Task<ActionResult> UpdateCliente(Usuarios usuario)
        {
            List<Usuarios> usuarios = new List<Usuarios>();
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

                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync($"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario actualizado correctamente.";

                    // IMPORTANTE: Actualiza la cookie si cambia el correo
                    Response.Cookies["UserEmail"].Value = usuario.Correo;

                    // Redirige al método que carga la vista con el modelo completo
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
        public async Task<ActionResult> CambiarContraseña(Usuarios usuario)
        {
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Encriptar la nueva contraseña antes de enviarla
            usuario.Contraseña = Encriptador.Encriptar(usuario.Contraseña);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync($"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Contraseña actualizada correctamente.";
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar la contraseña: {errorMessage}";
                    Console.WriteLine($"Error en API: {errorMessage}");

                    return View("EditarPerfilCliente");
                }
            }

            return RedirectToAction("Iniciar", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> AgregarRespuestasFAQ(RespuestasFAQ model)
        {
            try
            {
                // Validar si la pregunta no está vacía
                if (string.IsNullOrEmpty(model.Pregunta))
                {
                    TempData["Error"] = "Por favor, completa la pregunta.";
                    return RedirectToAction("RespuestasFAQ");
                }

                // Si la respuesta está vacía, asignamos null
                if (string.IsNullOrEmpty(model.Respuesta))
                {
                    model.Respuesta = null;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/RespuestasFAQ/PostRespuestaFAQ", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("RespuestasFAQ");
                    }
                }

                TempData["Success"] = "Pregunta frecuente agregada correctamente.";
                return RedirectToAction("RespuestasFAQ");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("RespuestasFAQ");
            }
        }
    }
}
