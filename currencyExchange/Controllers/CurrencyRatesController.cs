using currencyExchange.Services;
using currencyExchange.Services.ForexService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace currencyExchange.Controllers
{
    [JwtAuthentication]
    public class CurrencyRatesController : BaseController
    {
        private readonly IForexService _forexService;

        public CurrencyRatesController(IForexService forexService)
        {
            _forexService = forexService;
        }
        public async Task<IActionResult> LiveCurrencyUpdate()
        {
            try
            {
                var rates = await _forexService.GetForexRatesAsync();
                return View(rates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return View();
        }
    }
}
