﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AGONFRONT.Models;


namespace AGONFRONT.Controllers
{
    public class BusquedasController : Controller
    {

        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: Busquedas

        [HttpGet]
        public async Task<ActionResult> Busquedas(string busqueda)
        {
            List<Busquedas> producto = new List<Busquedas>();

            // Verificar si el token está en las cookies
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

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Hacer la petición GET
                    HttpResponseMessage response = await client.GetAsync($"api/Productos/GetBusqueda/{busqueda}");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = $"Error al obtener envíos (Código: {response.StatusCode}).";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res); // Esto imprimirá la respuesta JSON en la consola
                    producto = JsonConvert.DeserializeObject<List<Busquedas>>(res) ?? new List<Busquedas>();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(producto);
        }

    }
}
