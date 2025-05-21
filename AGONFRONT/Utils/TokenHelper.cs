using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;

namespace AGONFRONT.Utils
{
    public static class TokenHelper
    {
        public static int? GetUserIdFromToken(HttpContextBase httpContext)
        {
            var tokenCookie = httpContext.Request.Cookies["BearerToken"];
            var tokenSession = httpContext.Session["BearerToken"] as string;

            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");

            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }
    }
}
