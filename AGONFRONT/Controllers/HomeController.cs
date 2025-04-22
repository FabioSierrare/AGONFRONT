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

                                string hash = Encriptador.Encriptar(model.Contraseña);
                                System.Diagnostics.Debug.WriteLine("🔒 HASH GENERADO DESDE FRONT: " + hash);
                                model.Contraseña = hash;

                                string json = JsonConvert.SerializeObject(model);
                                System.Diagnostics.Debug.WriteLine("📤 JSON ENVIADO: " + json);

                                var content = new StringContent(json, Encoding.UTF8, "application/json");

                                // ⏱️ Enviar la solicitud POST a la API
                                HttpResponseMessage response = await client.PostAsync("api/Auth/Login", content);

                                // ✅ Si la respuesta fue exitosa
                                if (response.IsSuccessStatusCode)
                                {
                                    var res = await response.Content.ReadAsStringAsync();
                                    Token token = JsonConvert.DeserializeObject<Token>(res);

                                    // 🧠 Extraer tipo de usuario desde el token
                                    var tipousuario = GetUserTypeFromToken(token.token);

                                    if (token != null && !string.IsNullOrEmpty(token.token))
                                    {
                                        SetTokenCookie(token.token);
                                        SetUserEmailCookie(model.Correo);

                                        Session["RolUsuario"] = tipousuario;
                                        Session["BearerToken"] = token.token;

                                        if (tipousuario == "Vendedor")
                                            return RedirectToAction("UpdatePerfilVendedor", "Usuarios");

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

                    // Método para guardar el token JWT en una cookie
                    private void SetTokenCookie(string token)
                    {
                        var cookieOptions = new HttpCookie("BearerToken", token)
                        {
                            HttpOnly = true,      // Impide acceso JavaScript
                            Secure = true,        // Asegúrate de usar HTTPS
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.Now.AddMinutes(20)  // Duración del token
                        };

                        HttpContext.Response.Cookies.Add(cookieOptions);
                    }

                    // Método para guardar el email en una cookie
                    private void SetUserEmailCookie(string Correo)
                    {
                        var emailCookie = new HttpCookie("UserEmail", Correo)
                        {
                            HttpOnly = false,  // Permitir acceso desde JavaScript si es necesario
                            Secure = true,     // Asegurar que solo se envíe en HTTPS
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.Now.AddHours(1)  // Expira en 1 hora
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

                    public void SaveUserIdFromToken()
                    {
                        int? userId = TokenHelper.GetUserIdFromToken(HttpContext);

                        if (userId.HasValue)
                        {
                            var idCookie = new HttpCookie("UserId", userId.Value.ToString())
                            {
                                HttpOnly = true,  // Protección contra JavaScript
                                Secure = true,    // Solo en HTTPS
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTime.Now.AddHours(1)  // Expira en 1 hora
                            };

                            HttpContext.Response.Cookies.Add(idCookie);
                        }
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

                    public string GetUserTypeFromToken(string token)
                    {
                        if (string.IsNullOrEmpty(token))
                            return null;

                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);
                        var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TipoUsuario");

                        return userTypeClaim?.Value;
                    }


                    public ActionResult AccesoDenegado()
                    {
                        ViewBag.Mensaje = "No tienes permisos para acceder a esta sección.";
                        return View();
                    }

                    public ActionResult MisDatos()
                    {
                        if (Session["RolUsuario"] == null)
                        {
                            return RedirectToAction("Iniciar", "Home");
                        }

                        // continuar
                        return View();
                    }

                }
            }