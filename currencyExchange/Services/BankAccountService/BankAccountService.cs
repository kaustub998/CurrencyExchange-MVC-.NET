using currencyExchange.Models;
using currencyExchange.Services.ForexService;
using Microsoft.EntityFrameworkCore;

namespace currencyExchange.Services.BankAccountService
{
    public class BankAccountService : IBankAccountService
    {
        private readonly AppDbContext _context;
        private readonly IForexService _forexService;

        public BankAccountService(AppDbContext context, IForexService forexService)
        {
            _context = context;
            _forexService = forexService;
        }

        public async Task<List<BankAccount>> GetUserBankAccountsAsync(string userId)
        {
            return await _context.BankAccounts.Where(b => b.UserId == Convert.ToInt32(userId) && b.IsDeleted != true).ToListAsync();
        }

        public async Task<List<string>> GetAvailableCurrencies()
        {
            var rates = await _forexService.GetForexRatesAsync();
            return new List<string> {"NPR"}.Concat( rates.Select(item => item.Currency.Iso3).ToList()).ToList();
        }

        public async Task CreateBankAccountAsync(BankAccount account)
        {
            _context.BankAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBankAccountAsync(int accountId)
        {
            var account = await _context.BankAccounts.FindAsync(accountId);
            if (account != null)
            {
                account.IsDeleted = true;
                _context.BankAccounts.Update(account);
                await _context.SaveChangesAsync();
            }
        }
    }
}
