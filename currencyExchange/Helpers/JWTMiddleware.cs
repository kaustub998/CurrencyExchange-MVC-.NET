using currencyExchange.Services.JWTauthenticationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace currencyExchange.Services
{
    public class JwtAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Access the JWTService from HttpContext.RequestServices
            var jwtService = filterContext.HttpContext.RequestServices.GetService(typeof(JWTService)) as JWTService;

            var request = filterContext.HttpContext.Request;
            var token = request.Cookies["JWToken"];

            if (token != null)
            {
                var userName = jwtService.ValidateToken(token);
                if (userName == null)
                {
                    filterContext.Result = new UnauthorizedResult(); // 401 Unauthorized
                }
            }
            else
            {
                filterContext.Result = new UnauthorizedResult(); // 401 Unauthorized
            }

            base.OnActionExecuting(filterContext);
        }
    }

}
