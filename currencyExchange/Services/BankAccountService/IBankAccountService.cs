using currencyExchange.Models;

namespace currencyExchange.Services.BankAccountService
{
    public interface IBankAccountService
    {
        Task<List<BankAccount>> GetUserBankAccountsAsync(string userId);
        Task<List<string>> GetAvailableCurrencies();
        Task CreateBankAccountAsync(BankAccount account);
        Task DeleteBankAccountAsync(int accountId);
    }
}
