using AGONFRONT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace AGONFRONT.Controllers
{
    public class ProductosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public async Task<ActionResult> Productos()
        {
            List<Productos> productos = new List<Productos>();

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

            // Si la fecha de expiración ha pasado, eliminar la cookie y redirigir
            if (expirationTime.HasValue && DateTime.Now > expirationTime.Value)
            {
                HttpContext.Response.Cookies.Remove("BearerToken");
                HttpContext.Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Código para obtener productos de la API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }
            // Crear una cookie con las IDs de los productos
            var productosIds = productos.Select(p => p.Id).ToList(); // Obtener las IDs de los productos
            var productosIdsString = string.Join(",", productosIds); // Convertir las IDs a una cadena separada por comas

            // Crear la cookie para almacenar las IDs de los productos
            HttpCookie productosCookie = new HttpCookie("productosIds", productosIdsString)
            {
                Expires = DateTime.Now.AddHours(1),  // Establece la expiración de la cookie (1 hora)
                HttpOnly = true  // Asegura que la cookie solo sea accesible desde el servidor
            };

            // Guardar la cookie en la respuesta HTTP
            Response.Cookies.Add(productosCookie);

            return View(productos);

        }



        public async Task<ActionResult> Frutas()
        {
            List<Productos> frutas = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Asignamos la URL base al cliente HttpClient
                client.BaseAddress = new Uri(apiUrl);

                // Usamos la ruta relativa ahora
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    frutas = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            return View(frutas);

        }
        public async Task<ActionResult> Verduras()
        {
            List<Productos> verduras = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Asignamos la URL base al cliente HttpClient
                client.BaseAddress = new Uri(apiUrl);

                // Usamos la ruta relativa ahora
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    verduras = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            return View(verduras);

        }
        public async Task<ActionResult> Tuberculos()
        {
            List<Productos> tuberculos = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Asignamos la URL base al cliente HttpClient
                client.BaseAddress = new Uri(apiUrl);

                // Usamos la ruta relativa ahora
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    tuberculos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            return View(tuberculos);

        }

        // Acción para ver el listado de productos (por ejemplo)
        public async Task<ActionResult> Detalles(int id)
        {
            Productos producto = null;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl); // Asegúrate de que apiUrl esté bien configurado

                    // Hacemos la llamada a la API usando el endpoint correcto
                    HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        producto = JsonConvert.DeserializeObject<Productos>(res);
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo obtener el producto de la API. Código de estado: " + response.StatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al hacer la solicitud a la API: " + ex.Message;
            }

            // Si el producto no se encuentra o hubo un error, redirigir con un mensaje de error
            if (producto == null)
            {
                TempData["Error"] = "El producto no fue encontrado o hubo un problema con la conexión a la API.";
                return RedirectToAction("Productos");
            }

            return View(producto);
        }

        public async Task<ActionResult> Cereales()
        {
            List<Productos> cereal = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Asignamos la URL base al cliente HttpClient
                client.BaseAddress = new Uri(apiUrl);

                // Usamos la ruta relativa ahora
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    cereal = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los productos de la API.";
                }
            }

            return View(cereal);

        }
    }

}



