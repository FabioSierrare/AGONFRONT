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
        public async Task<ActionResult> Ofertas()
        {
            List<ProductosDescuentosDTO> viewModel = new List<ProductosDescuentosDTO>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("api/ProductosDescuento/GetProductosDescuentosExacto");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();

                        var productosDescuento = JsonConvert.DeserializeObject<List<ProductosDescuentosDTO>>(jsonData);

                        viewModel = productosDescuento;
                    }
                }

            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al conectar con el servidor. Intente más tarde.";
            }

            return View(viewModel);

        }

    }
}