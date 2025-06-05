using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AGONFRONT.Controllers
{
    public class TerminosyCondicionesController : Controller
    {
        // GET: TerminosyCondiciones
        //Returna los terminos y condiciones de nuestro proyecto
        public ActionResult TerminosyPoliticas()
        {
            return View();
        }
    }
}