using currencyExchange.Models;
using currencyExchange.Services.UserRegistrationService;
using Microsoft.AspNetCore.Mvc;
using XAct;

namespace currencyExchange.Controllers
{
    public class UserRegistrationLoginController : BaseController
    {
        private readonly IUserRegistrationLoginService _userRegistrationLoginService;

        public UserRegistrationLoginController(IUserRegistrationLoginService userRegistrationLoginService)
        {
            _userRegistrationLoginService = userRegistrationLoginService;
        }

        [HttpGet]
        public IActionResult RegisterUser()
        {
            bool isLoggedIn = HttpContext.Request.Cookies.ContainsKey("JWToken");
            return isLoggedIn? RedirectToAction("LiveCurrencyUpdate", "CurrencyRates") : View();
        }

        [HttpGet]
        public IActionResult LoginUser()
        {
            bool isLoggedIn = HttpContext.Request.Cookies.ContainsKey("JWToken");
            return isLoggedIn ? RedirectToAction("LiveCurrencyUpdate", "CurrencyRates") : View();
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
                    ViewData["isLoggedIn"] = true;
                    return RedirectToAction("LiveCurrencyUpdate", "CurrencyRates");
                }
                ModelState.AddModelError("", response.message);
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("JWToken");
            return RedirectToAction("LoginUser", "UserRegistrationLogin");
        }
    }
}
