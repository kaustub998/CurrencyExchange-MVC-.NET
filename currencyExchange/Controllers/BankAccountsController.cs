using currencyExchange.Models;
using currencyExchange.Services;
using currencyExchange.Services.BankAccountService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
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
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    account.UserId = Convert.ToInt32(userId);
                    var check = await _bankAccountService.CreateBankAccountAsync(account);
                    
                    if(check)
                        return RedirectToAction("AllBankAccounts");

                    ModelState.AddModelError("", "Same Account Number already exists.");
                }
            }
            catch (SqlException ex)
            {
               
            }

            var currencies = await _bankAccountService.GetAvailableCurrencies();
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
            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            var currencies = await _bankAccountService.GetAvailableCurrencies();
            ViewData["Currencies"] = currencies;

            if (!ModelState.IsValid)
            {
                ViewData["SendMoneyModal"] = true;
                return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            }

            var destinationAccount = await _bankAccountService.ValidateDestinationAccountAsync(
                model.DestinationAccountName,
                model.DestinationAccountNumber
            );

            if (destinationAccount == null)
            {
                ModelState.AddModelError("", "Invalid destination account details.");
                ViewData["SendMoneyModal"] = true;
                return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            }


            var sourceAccount = await _bankAccountService.CheckSourceAccount(model.SourceAccountId);

            if (sourceAccount == null)
            {
                ModelState.AddModelError("", "Invalid source account.");
                ViewData["SendMoneyModal"] = true;
                return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            }

            if (sourceAccount.Balance < model.Amount)
            {
                ModelState.AddModelError("", "Insufficient balance.");
                ViewData["SendMoneyModalOpen"] = true;
                return View("AllBankAccounts", await _bankAccountService.GetUserBankAccountsAsync(userId));
            }

            var transactionSuccess = await _bankAccountService.PerformTransaction(sourceAccount.AccountId, destinationAccount.AccountId, model.Amount,model.Remarks);

            if (!transactionSuccess)
            {
                TempData["ErrorMessage"] = "Transaction failed. Please check the account balances or currencies.";
            }
            else
            {
                TempData["SuccessMessage"] = "Transaction completed successfully.";
            }

            var userBankAccounts = await _bankAccountService.GetUserBankAccountsAsync(userId);
            return View("AllBankAccounts", userBankAccounts);
        }

        [HttpPost]
        public async Task<IActionResult> AddBalance(int accountId, decimal Amount,string Remarks)
        {
            var currencies = await _bankAccountService.GetAvailableCurrencies();
            ViewData["Currencies"] = currencies;

            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            if (Amount <= 0)
            {
                TempData["ErrorMessage"] = "Amount should be greater than 0.";
                return RedirectToAction("AllBankAccounts");
            }

            var check = await _bankAccountService.AddBalance(accountId, Amount,Remarks);

            if (check)
                return RedirectToAction("AllBankAccounts");
            
            ModelState.AddModelError("", "Some error occured.");
            return RedirectToAction("AllBankAccounts");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBalance(int accountId, decimal Amount, string Remarks)
        {
            var currencies = await _bankAccountService.GetAvailableCurrencies();
            ViewData["Currencies"] = currencies;

            var userId = JwtHelper.GetUserIdFromJwtCookie(HttpContext.Request.Cookies);

            if (Amount <= 0)
            {
                TempData["ErrorMessage"] = "Amount should be greater than 0.";
                return RedirectToAction("AllBankAccounts");
            }

            var check = await _bankAccountService.RemoveBalance(accountId, Amount, Remarks);

            if (check)
                return RedirectToAction("AllBankAccounts");

            ModelState.AddModelError("", "Some error occured.");
            return RedirectToAction("AllBankAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> AccountStatement(int accountId, DateTime fromDate, DateTime toDate)
        {
            var accountStatement = await _bankAccountService.GetAccountStatement(accountId, fromDate, toDate);
            return View(accountStatement);
        }

        [HttpGet]
        public async Task<IActionResult> TransactionDetail(int transactionId, int accountId, DateTime fromDate, DateTime toDate)
        {
            var selectedTransaction = await _bankAccountService.GetTransactionByIdAsync(transactionId);

            if (selectedTransaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction("AccountStatement", new { accountId, fromDate, toDate });
            }

            var accountStatement = await _bankAccountService.GetAccountStatement(accountId, fromDate, toDate);
            accountStatement.SelectedTransaction = selectedTransaction;

            return View("AccountStatement", accountStatement);
        }


    }
}
