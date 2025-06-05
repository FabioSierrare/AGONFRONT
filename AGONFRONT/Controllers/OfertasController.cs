using AGONFRONT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;

namespace AGONFRONT.Controllers
{
    public class OfertasController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: Descuentos
        /// <summary>
        /// Obtiene la lista de productos con descuentos exactos desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Una vista con la lista de productos con descuentos.</returns>
        public async Task<ActionResult> Ofertas()
        {
            List<ProductosDescuentosDTO> viewModel = new List<ProductosDescuentosDTO>();

            try
            {
                using (var client = new HttpClient())
                {
                    // Establece la URL base del cliente HTTP
                    client.BaseAddress = new Uri(apiUrl);

                    // Limpia las cabeceras Accept para asegurarse que se acepta JSON
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Solicita la lista de productos con descuentos desde la API
                    HttpResponseMessage response = await client.GetAsync("api/ProductosDescuento/GetProductosDescuentosExacto");

                    if (response.IsSuccessStatusCode)
                    {
                        // Lee la respuesta como cadena JSON
                        var jsonData = await response.Content.ReadAsStringAsync();

                        // Deserializa el JSON a una lista de ProductosDescuentosDTO
                        var productosDescuento = JsonConvert.DeserializeObject<List<ProductosDescuentosDTO>>(jsonData);

                        // Asigna la lista obtenida al modelo de vista
                        viewModel = productosDescuento;
                    }
                }

            }
            catch (Exception ex)
            {
                // En caso de error, muestra mensaje amigable al usuario
                ViewBag.ErrorMessage = "Ocurrió un error al conectar con el servidor. Intente más tarde.";
            }

            // Devuelve la vista con la lista de productos con descuentos
            return View(viewModel);
        }

    }
}