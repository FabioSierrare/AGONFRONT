using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AGONFRONT.Models;

namespace AGONFRONT.Controllers
{
    public class EstadisticasVentasController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public async Task<ActionResult> EstadisticasVentas()
        {
            // Verificar si el token está en las cookies o en la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Obtener el ID del usuario desde el token
            string userId = GetLoggedInUserId(token);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "No se pudo obtener la información del usuario.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Obtener los datos desde la API
            var ventasPorSemana = await GetVentasUltimasSemanas(userId, token);
            var productosMasVendidos = await GetProductosMasVendidos(userId, token);

            // Crear el modelo de la vista
            var viewModel = new Estadisticasx
            {
                VentasPorSemana = ventasPorSemana,
                ProductosMasVendidos = productosMasVendidos
            };

            return View(viewModel);
        }

        public async Task<List<VentasPorSemana>> GetVentasUltimasSemanas(string IdVendedor, string token)
        {
            List<VentasPorSemana> ventas = new List<VentasPorSemana>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync($"api/Estadisticas/GetVentasUltimasSemanas/{IdVendedor}");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = $"Error al obtener ventas (Código: {response.StatusCode}).";
                        return new List<VentasPorSemana>();
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res);
                    ventas = JsonConvert.DeserializeObject<List<VentasPorSemana>>(res) ?? new List<VentasPorSemana>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
            }

            return ventas;
        }

        public async Task<List<ProductosMasVendidos>> GetProductosMasVendidos(string IdVendedor, string token)
        {
            List<ProductosMasVendidos> productos = new List<ProductosMasVendidos>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync($"api/Estadisticas/GetProductosMasVendidos/{IdVendedor}");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = $"Error al obtener productos más vendidos (Código: {response.StatusCode}).";
                        return new List<ProductosMasVendidos>();
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res);
                    productos = JsonConvert.DeserializeObject<List<ProductosMasVendidos>>(res) ?? new List<ProductosMasVendidos>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
            }

            return productos;
        }

        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }
    }
}
