using Microsoft.AspNetCore.Mvc;

namespace currencyExchange.Controllers
{
    public class BankAccountsController : Controller
    {
        public IActionResult AllBankAccounts()
        {
            return View();
        }
    }
}
