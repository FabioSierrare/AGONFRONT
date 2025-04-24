using AGONFRONT.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;
using System.Text;

namespace AGONFRONT.Controllers
{
    public class RecuperacionController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        // Vista para ingresar correo electrónico para la recuperación de contraseña
        [HttpGet]
        public ActionResult RecuperarContraseña()
        {
            return View();
        }

        // Acción para enviar el correo de recuperación y llamar a la API
        [HttpPost]
        public async Task<ActionResult> EnviarRecuperacion(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var contenido = new StringContent(JsonConvert.SerializeObject(new { Correo = email }), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/Auth/RecuperarContraseña", contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Se ha enviado un código a tu correo.";
                        TempData["Correo"] = email; // guardamos el correo para usarlo en VerificarCodigo
                        return RedirectToAction("VerificarCodigo");
                    }
                    else
                    {
                        TempData["Mensaje"] = "El correo no está registrado o hubo un error.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al conectar con la API: {ex.Message}";
            }

            return RedirectToAction("RecuperarContraseña");
        }

        [HttpGet]
        public ActionResult VerificarCodigo()
        {
            ViewBag.Correo = TempData["Correo"];
            TempData.Keep("Correo"); // conservar el valor si se actualiza la página
            return View();
        }
        // Acción para verificar el código de recuperación
        [HttpPost]
        public async Task<ActionResult> VerificarCodigo(string email, string codigo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var contenido = new StringContent(JsonConvert.SerializeObject(new { Correo = email, Codigo = codigo }), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/Auth/VerificarCodigo", contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Código verificado correctamente.";
                        return RedirectToAction("NuevaContraseña", new { token = codigo });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Mensaje"] = errorContent;
                        TempData["Correo"] = email; // mantener el correo para reintento
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al conectar con la API: {ex.Message}";
                TempData["Correo"] = email;
            }

            return RedirectToAction("VerificarCodigo");
        }

        // Vista para restablecer la contraseña
        [HttpGet]
        public ActionResult NuevaContraseña(string token)
        {
            var modelo = new RestablecerContraseña
            {
                Codigo = token
            };

            return View(modelo);
        }

        // Acción para restablecer la contraseña
        [HttpPost]
        public async Task<ActionResult> NuevaContraseña(RestablecerContraseña modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    var json = JsonConvert.SerializeObject(modelo);
                    var contenido = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/Auth/RestablecerContraseña", contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Contraseña restablecida correctamente.";
                        return RedirectToAction("Iniciar", "Home");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Error: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al conectar con la API: {ex.Message}");
            }

            return View(modelo);
        }
    }
}