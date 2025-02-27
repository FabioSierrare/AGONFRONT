using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using AGONFRONT.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace AGONFRONT.Controllers
{
    public class HomeController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Iniciar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Models.Login model)
        {
            if (!ModelState.IsValid)
            {
                // Manejo de errores en el ModelState
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        TempData["Error"] = $"Campo: {key} - Error: {error.ErrorMessage}";
                    }
                }
                return RedirectToAction("Iniciar", "Home");
            }

            Token token = new Token();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Clear();

                string json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/Auth/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<Token>(res);

                    if (token != null && !string.IsNullOrEmpty(token.token))
                    {
                        // Guardamos el token JWT en una cookie
                        SetTokenCookie(token.token);

                        return RedirectToAction("Productos", "Productos");
                    }
                    else
                    {
                        TempData["Error"] = "Error al obtener el token.";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }
                else
                {
                    TempData["Error"] = "Credenciales incorrectas o problema con la API.";
                    return RedirectToAction("Iniciar", "Home");
                }
            }
        }

        // Método para guardar el token JWT en una cookie
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new HttpCookie("BearerToken", token)
            {
                HttpOnly = true,      // Impide acceso JavaScript
                Secure = true,        // Asegúrate de usar HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddSeconds(20)  // Duración del token
            };

            HttpContext.Response.Cookies.Add(cookieOptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // Eliminar las cookies de autenticación JWT
            HttpContext.Response.Cookies["BearerToken"].Expires = DateTime.Now.AddDays(-1);

            // Redirigir al usuario a la página de inicio de sesión
            return RedirectToAction("Iniciar", "Home");
        }
    }
}