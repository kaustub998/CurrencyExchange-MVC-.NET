using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace currencyExchange.Controllers
{
    [Authorize]
    public class CurrencyRatesController : Controller
    {
        public IActionResult LiveCurrencyUpdate()
        {
            return View();
        }
    }
}
