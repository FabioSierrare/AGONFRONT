using System.Web.Mvc;
using System.Web.Routing;
using System.Web;

public class AuthorizeByRoleAttribute : AuthorizeAttribute
{
    private readonly string _rolNecesario;

    public AuthorizeByRoleAttribute(string rolNecesario)
    {
        _rolNecesario = rolNecesario;
    }

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        var rolEnSesion = httpContext.Session["RolUsuario"] as string;
        if (string.IsNullOrEmpty(rolEnSesion))
            return false;

        return rolEnSesion == _rolNecesario;
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        var rolEnSesion = filterContext.HttpContext.Session["RolUsuario"] as string;
        if (string.IsNullOrEmpty(rolEnSesion))
        {
            // No está autenticado → va al login (p.ej. /Home/Iniciar)
            base.HandleUnauthorizedRequest(filterContext);
        }
        else
        {
            // Ya estaba autenticado pero no tiene el rol “3”
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "controller", "Home" },
                    { "action", "AccesoDenegado" }
                });
        }
    }
}
