using currencyExchange.Models;

namespace currencyExchange.Services.BankAccountService
{
    public interface IBankAccountService
    {
        Task<List<BankAccount>> GetUserBankAccountsAsync(string userId);
        Task<List<string>> GetAvailableCurrencies();
        Task<bool> CreateBankAccountAsync(BankAccount account);
        Task DeleteBankAccountAsync(int accountId);
        Task<BankAccount?> ValidateDestinationAccountAsync(string bankAccountName, string bankAccountNumber);
        Task<BankAccount?> CheckSourceAccount(int accountId);
        Task<bool> PerformTransaction(int sourceAccountId, int destinationAccountId, decimal amount, string Remarks);
        Task<bool> AddBalance(int accountId, decimal amount, string remarks);
        Task<bool> RemoveBalance(int accountId, decimal amount, string remarks);
        Task<AccountStatement> GetAccountStatement(int accountId, DateTime fromDate, DateTime toDate);
        Task<Transaction> GetTransactionByIdAsync(int transactionId);
    }
}
