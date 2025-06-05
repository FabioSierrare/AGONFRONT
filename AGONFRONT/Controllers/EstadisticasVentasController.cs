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

        /// <summary>
        /// Obtiene y muestra las estadísticas de ventas del usuario autenticado.
        /// </summary>
        /// <remarks>
        /// El método verifica que exista un token JWT en las cookies o en la sesión,
        /// obtiene el ID del usuario desde el token y luego consume dos servicios API
        /// para obtener las ventas por semana y los productos más vendidos.
        /// Finalmente, prepara el modelo para la vista y lo retorna.
        /// </remarks>
        /// <returns>
        /// Una vista con el modelo <see cref="Estadisticasx"/> que contiene las estadísticas.
        /// En caso de no haber token válido, redirige a la página de inicio de sesión.
        /// </returns>
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


        /// <summary>
        /// Obtiene la lista de ventas por semana para un vendedor específico desde la API.
        /// </summary>
        /// <param name="IdVendedor">El identificador del vendedor.</param>
        /// <param name="token">El token JWT para autenticación en la API.</param>
        /// <returns>Una lista de objetos <see cref="VentasPorSemana"/> con las ventas recientes por semana.
        /// En caso de error o fallo en la conexión, retorna una lista vacía.</returns>
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


        /// <summary>
        /// Obtiene la lista de los productos más vendidos para un vendedor específico desde la API.
        /// </summary>
        /// <param name="IdVendedor">El identificador del vendedor.</param>
        /// <param name="token">El token JWT para autenticación en la API.</param>
        /// <returns>Una lista de objetos <see cref="ProductosMasVendidos"/> con los productos más vendidos.
        /// En caso de error o fallo en la conexión, retorna una lista vacía.</returns>
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

        /// <summary>
        /// Extrae el identificador del usuario (UserId) desde un token JWT.
        /// </summary>
        /// <param name="token">El token JWT desde el cual se extraerá el UserId.</param>
        /// <returns>El valor del claim UserId si existe; de lo contrario, null.</returns>
        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }

    }
}
