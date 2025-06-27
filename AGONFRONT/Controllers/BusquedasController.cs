using Newtonsoft.Json;
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
            string token = tokenCookie?.Value ?? Session["BearerToken"] as string;

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
                    HttpResponseMessage response = await client.GetAsync($"api/Productos/GetBusqueda?palabra={busqueda}");

                    if (string.IsNullOrWhiteSpace(busqueda))
                    {
                        ViewBag.Mensaje = "Por favor, escribe una palabra clave para buscar.";
                        return View(producto); // lista vacía
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        ViewBag.Mensaje = "Ocurrió un error al intentar buscar los productos.";
                        return View(producto); // Mostrar la vista vacía con mensaje
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(res); // Esto imprimirá la respuesta JSON en la consola
                    producto = JsonConvert.DeserializeObject<List<Busquedas>>(res) ?? new List<Busquedas>();


                    if (producto.Count == 0)
                    {
                        ViewBag.Mensaje = "No se encontraron productos con ese nombre.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Busquedas", "Busquedas");
            }

            return View(producto);
        }


        [HttpGet]
        public async Task<ActionResult> FiltrarProductos(List<int> categorias, int? precioMin, int? precioMax)
        {
            List<Busquedas> productosFiltrados = new List<Busquedas>();

            var tokenCookie = Request.Cookies["BearerToken"];
            string token = tokenCookie != null ? tokenCookie.Value : (Session["BearerToken"] as string);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string query = string.Empty;

                if ((categorias == null || !categorias.Any()) && !precioMin.HasValue && !precioMax.HasValue)
                {
                    var response = await client.GetAsync("/api/Productos/GetBusqueda");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        productosFiltrados = JsonConvert.DeserializeObject<List<Busquedas>>(json);
                    }

                    return PartialView("_PartialProductosBusqueda", productosFiltrados);
                }

                if (categorias != null && categorias.Any())
                {
                    foreach (var categoria in categorias)
                    {
                        query = $"categoriaId={categoria}";

                        if (precioMin.HasValue)
                            query += $"&precioMin={precioMin.Value}";
                        if (precioMax.HasValue)
                            query += $"&precioMax={precioMax.Value}";

                        var response = await client.GetAsync($"/api/Productos/GetBusqueda?{query}");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var productos = JsonConvert.DeserializeObject<List<Busquedas>>(json);
                            productosFiltrados.AddRange(productos);
                        }
                    }

                    // Elimina duplicados
                    productosFiltrados = productosFiltrados
                        .GroupBy(p => p.CategoriaId)
                        .Select(g => g.First())
                        .ToList();
                }
                else
                {
                    query = "";
                    if (precioMin.HasValue)
                        query += $"precioMin={precioMin.Value}";
                    if (precioMax.HasValue)
                        query += $"{(query.Length > 0 ? "&" : "")}precioMax={precioMax.Value}";

                    var response = await client.GetAsync($"/api/Productos/GetBusqueda?{query}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        productosFiltrados = JsonConvert.DeserializeObject<List<Busquedas>>(json);
                    }
                }
            }

            return PartialView("_PartialProductosBusqueda", productosFiltrados);
        }

        public ActionResult FiltroCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            var tokenCookie = Request.Cookies["BearerToken"];
            string token = tokenCookie != null ? tokenCookie.Value : (Session["BearerToken"] as string);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.GetAsync("/api/Categorias/GetCategoria").Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(json);
                }
            }

            return PartialView("_PartialAsideCategorias", categorias);
        }

    }
}
