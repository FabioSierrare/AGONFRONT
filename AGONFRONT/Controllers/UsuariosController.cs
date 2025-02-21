using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;

namespace AGONFRONT.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: X
        public ActionResult RegistroVendedor()
        {
            return View();
        }

        // GET: X/Details/5
        public ActionResult Register()
        {
            return View();
        }

        // GET: X/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Usuarios model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Usuarios/PostUsuarios", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        TempData["Error"] = "Credenciales incorrectas o problema con la API.";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }

        // GET: X/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: X/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Usuarios model)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: X/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: X/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Usuarios model)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
