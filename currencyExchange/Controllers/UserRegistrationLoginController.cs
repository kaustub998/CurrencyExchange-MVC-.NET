using currencyExchange.Models;
using currencyExchange.Services;
using Microsoft.AspNetCore.Mvc;

namespace currencyExchange.Controllers
{
    public class UserRegistrationLoginController : Controller
    {
        private readonly IUserRegistrationLoginService _userRegistrationLoginService;

        public UserRegistrationLoginController(IUserRegistrationLoginService userRegistrationLoginService)
        {
            _userRegistrationLoginService = userRegistrationLoginService;
        }

        [HttpGet]
        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LoginUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegister registrationData)
        {
            if (ModelState.IsValid)
            {
                var response = await _userRegistrationLoginService.UserRegistration(registrationData);
                if (response.isSuccess == true)
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", response.message);
            }

            return View(registrationData);
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(UserLogin loginData)
        {
            if (ModelState.IsValid)
            {
                var response = await _userRegistrationLoginService.UserLogin(loginData);
                if (response.message == null)
                {
                    HttpContext.Response.Cookies.Append("JWToken", response.tokenId, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddHours(1) });
                    return RedirectToAction("LiveCurrencyUpdate", "CurrencyRates");
                }
                ModelState.AddModelError("", response.message);
            }

            return View();
        }
    }
}
