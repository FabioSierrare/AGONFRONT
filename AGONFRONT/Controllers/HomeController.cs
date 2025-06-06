using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using AGONFRONT.Utils;
using Newtonsoft.Json;
// NO necesitamos using de OWIN ni de Security.Cookies ni de Claims

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
        /// Realiza el proceso de inicio de sesión:
        ///   1) Valida modelo
        ///   2) Envía credenciales a la API
        ///   3) Obtiene token JWT y extrae tipo de usuario
        ///   4) Guarda tipo de usuario en Session["RolUsuario"]
        ///   5) Redirige según rol
        /// </summary>
        public async Task<ActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, redirigimos a Iniciar
                TempData["Error"] = "Complete todos los campos correctamente.";
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
                    model.Contraseña = hash;

                    // Serializa el modelo a JSON
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Llamada a la API de autenticación
                    HttpResponseMessage response = await client.PostAsync("api/Auth/Login", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "Credenciales incorrectas. Verifica tu correo o contraseña.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Si la API devolvió un token JWT válido:
                    var res = await response.Content.ReadAsStringAsync();
                    Token token = JsonConvert.DeserializeObject<Token>(res);

                    // Extrae el TipoUsuarioId del JWT (ej: "1", "2" o "3")
                    string tipoId = GetUserTypeFromToken(token.token);

                    if (string.IsNullOrEmpty(token.token) || string.IsNullOrEmpty(tipoId))
                    {
                        TempData["Error"] = "El token recibido es inválido.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Guarda el token, el correo y el rol en sesión

                    Session["BearerToken"] = token.token;
                    Session["UserEmail"]   = model.Correo;
                    Session["RolUsuario"]  = tipoId;        // ej: "3" para administrador

                    // Guarda también el JWT en cookie si lo deseas (opcional)
                    SetTokenCookie(token.token);

                    // Redirige según el tipo de usuario:
                    // 1 → Cliente, 2 → Vendedor, 3 → Administrador


                    switch (tipoId)
                    {
                        case "1":
                            // Cliente → redirige a lista de productos
                            return RedirectToAction("Productos", "Productos");
                        case "2":
                            // Vendedor → va a su perfil
                            return RedirectToAction("UpdatePerfilVendedor", "Usuarios");
                        case "3":
                            // Administrador → va al dashboard de administración
                            return RedirectToAction("Dashboard", "Admin");
                        default:
                            // Si por alguna razón no coincide, lo mandamos a login otra vez
                            TempData["Error"] = "Rol de usuario no reconocido.";
                            return RedirectToAction("Iniciar", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                // Conexión fallida u otro error
                TempData["Error"] = "No se pudo conectar con el servidor. Intenta más tarde.";
                return RedirectToAction("Iniciar", "Home");
            }
        }

        /// <summary>
        /// Guarda el token JWT en una cookie HTTP segura con HttpOnly.
        /// Esto es opcional: sirve si quieres leer el JWT desde JavaScript o reenviarlo
        /// en otras peticiones. Si no te hace falta cookie, puedes omitir este método
        /// y eliminar la llamada en Login().
        /// </summary>
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new HttpCookie("BearerToken", token)
            {
                HttpOnly = true,      // Impide acceso desde JavaScript
                Secure = true,        // Solo enviar sobre HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddMinutes(20)
            };
            Response.Cookies.Add(cookieOptions);
        }

        /// <summary>
        /// Extrae el tipo de usuario (claim "TipoUsuarioId") del JWT.
        /// </summary>
        public string GetUserTypeFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            // Busca el claim cuyo Type sea "TipoUsuarioId"
            var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TipoUsuarioId");
            return userTypeClaim?.Value;
        }

        /// <summary>
        /// Acción para cerrar sesión: limpia sesión y cookies.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // Limpiar toda la sesión
            Session.Clear();
            Session.Abandon();

            // Eliminar la cookie del token si existe
            if (Request.Cookies["BearerToken"] != null)
            {
                var tokenCookie = new HttpCookie("BearerToken")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = true
                };
                Response.Cookies.Add(tokenCookie);
            }

            // Eliminar la cookie del correo de usuario
            if (Request.Cookies["UserEmail"] != null)
            {
                var emailCookie = new HttpCookie("UserEmail")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Secure = true
                };
                Response.Cookies.Add(emailCookie);
            }

            // Redireccionar a la página de login
            return RedirectToAction("Iniciar", "Home");
        }

        /// <summary>
        /// Muestra la vista de acceso denegado si el usuario no tiene el rol apropiado.
        /// </summary>
        public ActionResult AccesoDenegado()
        {
            ViewBag.Mensaje = "No tienes permisos para acceder a esta sección.";
            return View();
        }

        /// <summary>
        /// Muestra los datos personales. Solo si Session["RolUsuario"] existe.
        /// </summary>
        public ActionResult MisDatos()
        {
            if (Session["RolUsuario"] == null)
            {
                return RedirectToAction("Iniciar", "Home");
            }

            return View();
        }
    }
}
