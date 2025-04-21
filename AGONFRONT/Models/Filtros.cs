using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AGONFRONT.Filters
{
    public class AuthorizeByRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeByRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // ✅ Verifica si hay sesión
            if (session == null || session["RolUsuario"] == null)
            {
                // 🔒 Redirige al login si no hay sesión activa
                filterContext.Result = new RedirectResult("~/Home/Iniciar");
                return;
            }

            string currentUserRole = session["RolUsuario"].ToString();

            // ❌ Si el rol no es uno de los permitidos
            if (!_allowedRoles.Contains(currentUserRole, StringComparer.OrdinalIgnoreCase))
            {
                // 🚫 Redirige a página de acceso denegado o al home
                filterContext.Result = new RedirectResult("~/Home/AccesoDenegado");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
