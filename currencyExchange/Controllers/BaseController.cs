using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace currencyExchange.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool isLoggedIn = HttpContext.Request.Cookies.ContainsKey("JWToken");
            ViewData["isLoggedIn"] = isLoggedIn;
            base.OnActionExecuting(context);
        }
    }
}
