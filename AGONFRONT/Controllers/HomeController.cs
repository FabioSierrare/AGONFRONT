            using System;
            using System.Configuration;
            using System.IdentityModel.Tokens.Jwt;
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
            using AGONFRONT.Utils;
            

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
        /// <summary>
        /// Realiza el proceso de inicio de sesión validando el modelo, 
        /// enviando las credenciales a la API de autenticación y gestionando el token JWT.
        /// </summary>
        /// <param name="model">Modelo que contiene el correo y contraseña del usuario.</param>
        /// <returns>Redirecciona a la vista correspondiente según el tipo de usuario o muestra errores.</returns>
        public async Task<ActionResult> Login(Models.Login model)
        {
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

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    // Encripta la contraseña antes de enviarla
                    string hash = Encriptador.Encriptar(model.Contraseña);
                    System.Diagnostics.Debug.WriteLine("🔒 HASH GENERADO DESDE FRONT: " + hash);
                    model.Contraseña = hash;

                    // Serializa el modelo a JSON para enviarlo en la solicitud POST
                    string json = JsonConvert.SerializeObject(model);
                    System.Diagnostics.Debug.WriteLine("📤 JSON ENVIADO: " + json);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Enviar la solicitud POST a la API para autenticar
                    HttpResponseMessage response = await client.PostAsync("api/Auth/Login", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        Token token = JsonConvert.DeserializeObject<Token>(res);

                        // Extrae el tipo de usuario del token JWT recibido
                        var tipousuario = GetUserTypeFromToken(token.token);

                        if (token != null && !string.IsNullOrEmpty(token.token))
                        {
                            // Guarda el token y email en cookies
                            SetTokenCookie(token.token);
                            SetUserEmailCookie(model.Correo);

                            // Guarda tipo de usuario y token en sesión
                            Session["RolUsuario"] = tipousuario;
                            Session["BearerToken"] = token.token;

                            // Redirige según el tipo de usuario
                            if (tipousuario == "Vendedor")
                                return RedirectToAction("Dashboard", "Usuarios");

                            if (tipousuario == "Cliente")
                                return RedirectToAction("Productos", "Productos");

                            return RedirectToAction("Productos", "Productos");
                        }
                        else
                        {
                            TempData["Error"] = "El token recibido es inválido.";
                            return RedirectToAction("Iniciar", "Home");
                        }
                    }
                    else
                    {
                        var errResponse = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine("❌ Error de API: " + errResponse);
                        TempData["Error"] = "Credenciales incorrectas. Verifica tu correo o contraseña.";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 Excepción al conectar con la API: " + ex.Message);
                TempData["Error"] = "No se pudo conectar con el servidor. Intenta más tarde.";
                return RedirectToAction("Iniciar", "Home");
            }
        }

        /// <summary>
        /// Guarda el token JWT en una cookie HTTP segura con configuración HttpOnly.
        /// </summary>
        /// <param name="token">Token JWT que será guardado en la cookie.</param>
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new HttpCookie("BearerToken", token)
            {
                HttpOnly = true,      // Impide acceso desde JavaScript
                Secure = true,        // Solo enviar sobre HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddMinutes(20)  // Expiración en 20 minutos
            };

            HttpContext.Response.Cookies.Add(cookieOptions);
        }

        /// <summary>
        /// Guarda el correo electrónico del usuario en una cookie con acceso desde JavaScript permitido.
        /// </summary>
        /// <param name="Correo">Correo electrónico del usuario.</param>
        private void SetUserEmailCookie(string Correo)
        {
            var emailCookie = new HttpCookie("UserEmail", Correo)
            {
                HttpOnly = false,  // Permite acceso desde JavaScript
                Secure = true,     // Solo enviar sobre HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddHours(1)  // Expiración en 1 hora
            };

            HttpContext.Response.Cookies.Add(emailCookie);
        }

        public static class TokenHelper
                    {
                        public static int? GetUserIdFromToken(HttpContextBase httpContext)
                        {
                            var tokenCookie = httpContext.Request.Cookies["BearerToken"];
                            var tokenSession = httpContext.Session["BearerToken"] as string;

                            string token = tokenCookie?.Value ?? tokenSession;

                            if (string.IsNullOrEmpty(token))
                                return null;

                            var handler = new JwtSecurityTokenHandler();
                            var jwtToken = handler.ReadJwtToken(token);
                            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");

                            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
                        }
                    }

        /// <summary>
        /// Extrae el ID del usuario del token JWT almacenado en la solicitud HTTP
        /// y guarda este ID en una cookie segura para uso posterior.
        /// </summary>
        public void SaveUserIdFromToken()
        {
            int? userId = TokenHelper.GetUserIdFromToken(HttpContext);

            if (userId.HasValue)
            {
                var idCookie = new HttpCookie("UserId", userId.Value.ToString())
                {
                    HttpOnly = true,  // Protege contra acceso desde JavaScript
                    Secure = true,    // Solo se envía en conexiones HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddHours(1)  // Expira en 1 hora
                };

                HttpContext.Response.Cookies.Add(idCookie);
            }
        }

        /// <summary>
        /// Acción que cierra la sesión del usuario limpiando la sesión y eliminando las cookies relacionadas con autenticación.
        /// </summary>
        /// <returns>Redirecciona a la página de inicio de sesión.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // Limpiar completamente la sesión
            Session.Clear();
            Session.Abandon();

            // Eliminar manualmente la cookie del token
            if (Request.Cookies["BearerToken"] != null)
            {
                var tokenCookie = new HttpCookie("BearerToken")
                {
                    Expires = DateTime.Now.AddDays(-1),  // Expira inmediatamente
                    HttpOnly = true,
                    Secure = true
                };
                Response.Cookies.Add(tokenCookie);
            }

            // Eliminar manualmente la cookie del correo de usuario
            if (Request.Cookies["UserEmail"] != null)
            {
                var emailCookie = new HttpCookie("UserEmail")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Secure = true
                };
                Response.Cookies.Add(emailCookie);
            }

            // Eliminar manualmente la cookie del ID de usuario
            if (Request.Cookies["UserId"] != null)
            {
                var idCookie = new HttpCookie("UserId")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Secure = true
                };
                Response.Cookies.Add(idCookie);
            }

            // Redireccionar a la página de inicio de sesión
            return RedirectToAction("Iniciar", "Home");
        }



        /// <summary>
        /// Extrae el tipo de usuario del token JWT proporcionado.
        /// </summary>
        /// <param name="token">El token JWT del que se extraerá el tipo de usuario.</param>
        /// <returns>El valor del tipo de usuario si existe, o null si no se encuentra o el token es nulo o vacío.</returns>
        public string GetUserTypeFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TipoUsuario");

            return userTypeClaim?.Value;
        }

        /// <summary>
        /// Muestra la vista de acceso denegado con un mensaje explicativo.
        /// </summary>
        /// <returns>La vista que indica que el usuario no tiene permisos para acceder.</returns>
        public ActionResult AccesoDenegado()
        {
            ViewBag.Mensaje = "No tienes permisos para acceder a esta sección.";
            return View();
        }

        /// <summary>
        /// Muestra la vista de los datos personales del usuario.
        /// Si no hay sesión activa o el rol no está definido, redirige al login.
        /// </summary>
        /// <returns>La vista de datos personales o redirección al login.</returns>
        public ActionResult MisDatos()
        {
            if (Session["RolUsuario"] == null)
            {
                return RedirectToAction("Iniciar", "Home");
            }

            // continuar con la lógica para mostrar datos del usuario
            return View();
        }


    }
}