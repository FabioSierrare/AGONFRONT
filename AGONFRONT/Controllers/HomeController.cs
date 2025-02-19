using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;
using System.Web.Security;
using System.Net.Http;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

namespace AGONFRONT.Controllers
{
    public class HomeController : Controller
    {
        string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public ActionResult Index() //Clase genérica de retorno (osea que admite lo que sea)
        {
            return View();
        }

        public ActionResult LoginPartial()
        {
            return PartialView("_LoginPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Usuarios model)
        {
            string returnUrl = Url.Action("Index", "Home");
            Token token = new Token();

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Clear();
                string json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage Res = await client.PostAsync("api/Auth/Login", content);

                if (Res.IsSuccessStatusCode)
                {
                    var res = Res.Content.ReadAsStringAsync().Result;
                    token = JsonConvert.DeserializeObject<Token>(res);
                    CookieUpdate(model);
                    Session["BearerToken"] = token.token;

                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private void CookieUpdate(Usuarios usuario)
        {
            var ticket = new FormsAuthenticationTicket(2,
                usuario.Correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                false,
                JsonConvert.SerializeObject(usuario)
            );
            Session["Username"] = usuario.Correo;
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) { };
            Response.AppendCookie(cookie);
        }
    }
}