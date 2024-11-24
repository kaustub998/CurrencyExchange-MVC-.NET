using currencyExchange.Models;
using currencyExchange.Services;
using currencyExchange.Services.BankAccountService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using XAct.Users;

namespace currencyExchange.Controllers
{
    [JwtAuthentication]
    public class BankAccountsController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;
        public BankAccountsController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> AllBankAccounts()
        {
            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var bankAccounts = await _bankAccountService.GetUserBankAccountsAsync(userId);
            var currencies = await _bankAccountService.GetAvailableCurrencies();

            ViewData["Currencies"] = currencies;

            return View(bankAccounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllBankAccounts(BankAccount account)
        {
            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    account.UserId = Convert.ToInt32(userId);
                    await _bankAccountService.CreateBankAccountAsync(account);
                    return RedirectToAction("AllBankAccounts"); 
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    ModelState.AddModelError("DuplicateKey", "An account with this number already exists.");
                }
                else
                {
                    ModelState.AddModelError("Error", "An unexpected error occurred. Please try again later.");
                }
            }

            var currencies = _bankAccountService.GetAvailableCurrencies();
            ViewData["Currencies"] = currencies;
            return View(await _bankAccountService.GetUserBankAccountsAsync(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            await _bankAccountService.DeleteBankAccountAsync(accountId);

            return RedirectToAction("AllBankAccounts");
        }

        [HttpPost]
        public async Task<IActionResult> SendMoneyStep1(SendMoneyModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);
            //    ViewData["SendMoneyModalOpen"] = true;
            //    return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            //}

            //// Example validation logic
            //var destinationAccount = await _bankAccountService.ValidateDestinationAccountAsync(
            //    model.DestinationAccountName,
            //    model.DestinationAccountNumber
            //);

            //if (destinationAccount == null)
            //{
            //    ModelState.AddModelError("", "Invalid destination account details.");
            //    var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);
            //    ViewData["SendMoneyModalOpen"] = true;
            //    return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            //}

            ViewData["SendMoneyModalStep2Open"] = true;
            //ViewData["SourceAccountId"] = model.SourceAccountId;
            //ViewData["DestinationAccountId"] = destinationAccount.AccountId;

            var userBankAccounts = await _bankAccountService.GetUserBankAccountsAsync(
                JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies)
            );

            return View("AllBankAccounts", userBankAccounts);
        }
    }
}
