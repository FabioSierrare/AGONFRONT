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

            using (var client = new HttpClient())
            {
                // Asignamos la URL base al cliente HttpClient
                client.BaseAddress = new Uri(apiUrl);

                // Usamos la ruta relativa ahora
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

            return View(productos);
        }
    }
}


