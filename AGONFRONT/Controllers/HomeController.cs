﻿using System;
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
                Console.WriteLine("Errores en ModelState:");

                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        TempData["Error"] = ($"Campo: {key} - Error: {error.ErrorMessage}");
                    }
                }
                TempData["ErrorDetalle"] = $"Email recibido: {model.Correo} - Password recibido: {model.Contraseña}";
                return RedirectToAction("Iniciar", "Home");
            }



            Console.WriteLine($"Email recibido: {model.Correo}");
            Console.WriteLine($"Password recibido: {model.Contraseña}");


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
                        CookieUpdate(model);
                        Session["BearerToken"] = token.token;
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

        // Método para guardar el token en una cookie
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new HttpCookie("BearerToken", token)
            {
                HttpOnly = true,            // No accesible desde JavaScript
                Secure = false,              // Solo se enviará a través de HTTPS
                SameSite = SameSiteMode.Strict,  // Restricción para evitar el envío en solicitudes cross-origin
                Expires = DateTime.Now.AddMinutes(1) // Duración de la cookie (10 minutos en este caso)
            };

            // Agregar la cookie de BearerToken
            HttpContext.Response.Cookies.Add(cookieOptions);

            // Establecer la cookie de expiración
            var expirationCookie = new HttpCookie("TokenExpirationTime", DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"))
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict
            };

            // Agregar la cookie de expiración
            HttpContext.Response.Cookies.Add(expirationCookie);
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Iniciar", "Home");
        }

        private void CookieUpdate(Models.Login usuario)
        {
            var ticket = new FormsAuthenticationTicket(
                2,
                usuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                false,
                JsonConvert.SerializeObject(usuario)
            );

            Session["Username"] = usuario.Correo;

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL
            };

            Response.AppendCookie(cookie);
        }
    }
}