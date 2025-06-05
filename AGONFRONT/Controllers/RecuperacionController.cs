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
        /// <summary>
        /// Envía una solicitud para recuperar la contraseña enviando un código al correo proporcionado.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario que desea recuperar su contraseña.</param>
        /// <returns>Redirige a la vista de verificación de código si tiene éxito, o a la página de recuperación con mensaje de error.</returns>
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
        /// <summary>
        /// Muestra la vista para que el usuario ingrese el código recibido por correo.
        /// </summary>
        /// <returns>La vista de verificación de código con el correo almacenado en ViewBag.</returns>
        public ActionResult VerificarCodigo()
        {
            ViewBag.Correo = TempData["Correo"];
            TempData.Keep("Correo"); // conservar el valor si se actualiza la página
            return View();
        }

        // Acción para verificar el código de recuperación
        [HttpPost]
        /// <summary>
        /// Verifica el código de recuperación enviado por correo para el usuario.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario que solicita recuperación.</param>
        /// <param name="codigo">Código enviado al correo para validar la recuperación.</param>
        /// <returns>
        /// Redirige a la vista de creación de nueva contraseña si el código es válido,
        /// o regresa a la verificación mostrando un mensaje de error.
        /// </returns>
        public async Task<ActionResult> VerificarCodigo(string email, string codigo)
        {
            // Validar que el correo y el código no estén vacíos o nulos
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(codigo))
            {
                TempData["Mensaje"] = "Correo y código son obligatorios.";
                return RedirectToAction("VerificarCodigo");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    // Serializar el correo y código en formato JSON para enviar a la API
                    var contenido = new StringContent(
                        JsonConvert.SerializeObject(new { Correo = email, Codigo = codigo }),
                        Encoding.UTF8,
                        "application/json");

                    // Enviar petición POST para validar el código
                    var response = await client.PostAsync("api/Auth/VerificarCodigo", contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        // Si es exitoso, mostrar mensaje y redirigir a nueva contraseña
                        TempData["Mensaje"] = "Código verificado correctamente.";
                        return RedirectToAction("NuevaContraseña", new { token = codigo });
                    }
                    else
                    {
                        // Si no, obtener mensaje de error de la API y mantener el correo para reintento
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Mensaje"] = errorContent;
                        TempData["Correo"] = email;
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción y mostrar mensaje de error
                TempData["Mensaje"] = $"Error al conectar con la API: {ex.Message}";
                TempData["Correo"] = email;
            }

            // En caso de error o fallo, regresar a la vista de verificación con mensaje
            return RedirectToAction("VerificarCodigo");
        }


        // Vista para restablecer la contraseña
        [HttpGet]
        /// <summary>
        /// Muestra la vista para que el usuario ingrese una nueva contraseña.
        /// </summary>
        /// <param name="token">Código/token de recuperación recibido por correo.</param>
        /// <returns>Vista con el modelo que contiene el código de recuperación.</returns>
        public ActionResult NuevaContraseña(string token)
        {
            var modelo = new RestablecerContraseña
            {
                Codigo = token
            };

            return View(modelo);
        }

        /// <summary>
        /// Procesa la solicitud para restablecer la contraseña del usuario.
        /// </summary>
        /// <param name="modelo">Modelo que contiene el código de recuperación y la nueva contraseña.</param>
        /// <returns>
        /// Redirige a la página de inicio de sesión si la contraseña se restablece correctamente,
        /// o vuelve a mostrar el formulario con los errores en caso contrario.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> NuevaContraseña(RestablecerContraseña modelo)
        {
            // Validar que el modelo sea válido (por ejemplo, que las contraseñas coincidan)
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);

                    // Serializar el modelo en JSON para enviarlo a la API
                    var json = JsonConvert.SerializeObject(modelo);
                    var contenido = new StringContent(json, Encoding.UTF8, "application/json");

                    // Enviar la petición POST para restablecer la contraseña
                    var response = await client.PostAsync("api/Auth/RestablecerContraseña", contenido);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Contraseña restablecida correctamente.";
                        return RedirectToAction("Iniciar", "Home");
                    }
                    else
                    {
                        // Obtener el mensaje de error devuelto por la API y agregarlo al modelo
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Error: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción y agregar mensaje de error al modelo
                ModelState.AddModelError(string.Empty, $"Error al conectar con la API: {ex.Message}");
            }

            // En caso de error, regresar a la vista con el modelo para mostrar mensajes
            return View(modelo);
        }

    }
}