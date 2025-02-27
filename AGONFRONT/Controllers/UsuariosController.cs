using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;

namespace AGONFRONT.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: X
        public ActionResult Register()
        {
            // Verificar si el token está presente en las cookies o en la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            // Verificar si existe el token en las cookies o la sesión
            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home"); // Redirige al login
            }

            // Si el token está presente en la cookie o sesión, se permite el acceso
            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Si el token es válido, renderiza la vista
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

                    HttpResponseMessage response = await client.PostAsync("api/Usuarios/GetUsuarios", content);

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

        public async Task<ActionResult> Usuarios()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            // Verificar si el token está presente en las cookies o la sesión
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

            // Si el token es válido, proceder a obtener los usuarios de la API
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los usuarios de la API.";
                }
            }

            // Obtener el correo y contraseña del usuario logueado (se espera que vengan de las cookies o la sesión)
            var correoLogueado = Session["Correo"] as string;
            var contrasenaLogueada = Session["Contraseña"] as string;

            // Filtrar usuarios por el correo y contraseña proporcionados
            var usuarioLogueado = usuarios
                .FirstOrDefault(u => u.Correo == correoLogueado && u.Contraseña == contrasenaLogueada);

            if (usuarioLogueado == null)
            {
                TempData["Error"] = "No se encontró el usuario con esas credenciales.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Devolver el usuario encontrado al frontend
            return View(usuarioLogueado);
        }

        // GET: X/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: X/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Usuarios model)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: X/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: X/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Usuarios model)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


    }
}
