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
    }
}