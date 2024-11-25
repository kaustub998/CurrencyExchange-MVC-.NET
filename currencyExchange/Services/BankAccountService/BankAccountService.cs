using currencyExchange.Models;
using currencyExchange.Services.ForexService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using XAct;

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
            return new List<string> { "NPR" }.Concat(rates.Select(item => item.Currency.Iso3).ToList()).ToList();
        }

        public async Task<bool> CreateBankAccountAsync(BankAccount account)
        {
            if(!_context.BankAccounts.Exists(item => item.AccountNumber == account.AccountNumber))
            {
                _context.BankAccounts.Add(account);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
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

        public async Task<BankAccount?> ValidateDestinationAccountAsync(string bankAccountName, string bankAccountNumber)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(item =>
                    item.BankAccountUserName!.ToLower().Trim() == bankAccountName.ToLower().Trim() &&
                    item.AccountNumber!.Trim() == bankAccountNumber.Trim());
        }

        public async Task<BankAccount?> CheckSourceAccount(int accountId)
        {
            return await _context.BankAccounts.FirstOrDefaultAsync(item => item.AccountId == accountId && item.IsDeleted != true);
        }

        public async Task<bool> PerformTransaction(int sourceAccountId, int destinationAccountId, decimal amount,string Remarks)
        {
            try
            {
                var sourceAccount = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountId == sourceAccountId && !a.IsDeleted);
                var destinationAccount = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountId == destinationAccountId && !a.IsDeleted);

                decimal convertedAmount = amount;
                decimal exchangeRate = 1;

                if (!string.Equals(sourceAccount?.Currency, destinationAccount?.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    convertedAmount = await ConvertCurrency(amount, sourceAccount.Currency, destinationAccount.Currency);
                    if (convertedAmount <= 0)
                    {
                        throw new InvalidOperationException("Currency conversion failed.");
                    }
                    exchangeRate = convertedAmount / amount;
                }

                if (sourceAccount.Balance < amount)
                {
                    throw new InvalidOperationException("Insufficient balance.");
                }

                sourceAccount.Balance -= amount;
                destinationAccount.Balance += convertedAmount;

                _context.BankAccounts.Update(sourceAccount);
                _context.BankAccounts.Update(destinationAccount);

                await RecordTransactionDetailsAsync(sourceAccount,destinationAccount,amount,convertedAmount,exchangeRate, Remarks);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private async Task RecordTransactionDetailsAsync(BankAccount? sourceAccount, BankAccount? destinationAccount, decimal amount, decimal convertedAmount, decimal exchangeRate, string Remarks, bool isCredit = false, bool isDebit = false)
        {
            try
            {
                if (isCredit)
                {
                    var oldBalance = destinationAccount.Balance - convertedAmount;

                    var transactionDetails = new Transaction
                    {
                        SenderAccountId = null,
                        ReceiverAccountId = destinationAccount.AccountId,
                        SenderCurrency = null,
                        ReceiverCurrency = destinationAccount.Currency,
                        SenderAmount = 0,
                        ReceiverAmount = convertedAmount,
                        ExchangeRate = exchangeRate,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        CalculationFormula = $"{convertedAmount} credited to account",
                        Remarks = Remarks
                    };

                    _context.TransactionDetails.Add(transactionDetails);

                    var transactionSummaryDestination = new TransactionSummary
                    {
                        AccountId = destinationAccount.AccountId,
                        Amount = convertedAmount,
                        Credited = true,
                        Debited = false,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        Remarks = Remarks,
                        OldBalance = oldBalance,
                    };

                    _context.TransactionSummary.Add(transactionSummaryDestination);
                }
                else if (isDebit)
                {
                    var oldBalance = sourceAccount.Balance + amount;

                    var transactionDetails = new Transaction
                    {
                        SenderAccountId = sourceAccount.AccountId,
                        ReceiverAccountId = null,            
                        SenderCurrency = sourceAccount.Currency,
                        ReceiverCurrency = null,            
                        SenderAmount = amount,
                        ReceiverAmount = 0,                 
                        ExchangeRate = exchangeRate,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        CalculationFormula = $"{amount} withdrawn from account",
                        Remarks = Remarks
                    };

                    _context.TransactionDetails.Add(transactionDetails);

                    var transactionSummarySource = new TransactionSummary
                    {
                        AccountId = sourceAccount.AccountId,
                        Amount = amount,
                        Credited = false,
                        Debited = true,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        Remarks = Remarks,
                        OldBalance = oldBalance
                    };

                    _context.TransactionSummary.Add(transactionSummarySource);
                }
                else
                {
                    var sourceOldBalance = sourceAccount.Balance + amount;
                    var destinationOldBalance = destinationAccount.Balance - convertedAmount;

                    var transactionDetails = new Transaction
                    {
                        SenderAccountId = sourceAccount.AccountId,
                        ReceiverAccountId = destinationAccount.AccountId,
                        SenderCurrency = sourceAccount.Currency,
                        ReceiverCurrency = destinationAccount.Currency,
                        SenderAmount = amount,
                        ReceiverAmount = convertedAmount,
                        ExchangeRate = exchangeRate,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        CalculationFormula = $"{amount} * {exchangeRate} = {convertedAmount}",
                        Remarks = Remarks
                    };

                    _context.TransactionDetails.Add(transactionDetails);
                    await _context.SaveChangesAsync();

                    var transactionSummarySource = new TransactionSummary
                    {
                        AccountId = sourceAccount.AccountId,
                        Amount = amount,
                        Credited = false,
                        Debited = true,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        Remarks = Remarks,
                        OldBalance = sourceOldBalance,
                        IsForeignTransaction = true,
                        TransactionDetailId = transactionDetails.TransactionId
                    };

                    _context.TransactionSummary.Add(transactionSummarySource);

                    var transactionSummaryDestination = new TransactionSummary
                    {
                        AccountId = destinationAccount.AccountId,
                        Amount = convertedAmount,
                        Credited = true,
                        Debited = false,
                        TransactionDate = DateTime.UtcNow,
                        TransactionStatus = "Completed",
                        Remarks = Remarks,
                        OldBalance = destinationOldBalance,
                        IsForeignTransaction = true,
                        TransactionDetailId = transactionDetails.TransactionId
                    };

                    _context.TransactionSummary.Add(transactionSummaryDestination);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while recording the transaction: {ex.Message}");
                throw;
            }
        }

        private async Task<decimal> ConvertCurrency(decimal amount, string sourceCurrency, string targetCurrency)
        {
            var conversionRate = await GetCurrencyConversionRateAsync(sourceCurrency, targetCurrency);

            if (conversionRate <= 0)
            {
                return 0;
            }

            return amount * conversionRate;
        }

        private async Task<decimal> GetCurrencyConversionRateAsync(string sourceCurrency, string targetCurrency)
        {
            if (sourceCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            var forexRates = await _forexService.GetForexRatesAsync();

            var sourceRate = forexRates.FirstOrDefault(rate =>
                rate.Currency.Iso3.Equals(sourceCurrency, StringComparison.OrdinalIgnoreCase));

            var targetRate = forexRates.FirstOrDefault(rate =>
                rate.Currency.Iso3.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase));

            if (targetCurrency.Equals("NPR", StringComparison.OrdinalIgnoreCase))
            {
                return sourceRate.Sell / sourceRate.Currency.Unit ?? throw new InvalidOperationException("Sell rate is not available for source currency.");
            }
            else if (sourceCurrency.Equals("NPR", StringComparison.OrdinalIgnoreCase))
            {
                return 1 / (targetRate.Buy / targetRate.Currency.Unit ?? throw new InvalidOperationException("Buy rate is not available for target currency."));
            }
            return 1;
        }

        public async Task<bool> AddBalance(int accountId, decimal amount,string remarks)
        {
            try
            {
                var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountId == accountId && !a.IsDeleted);

                account.Balance += amount;
                _context.BankAccounts.Update(account);
                await _context.SaveChangesAsync();

                await RecordTransactionDetailsAsync(null,account,0,amount,0, remarks,true);
            }
            catch { }
            return false;
        }

        public async Task<bool> RemoveBalance(int accountId, decimal amount, string remarks)
        {
            try
            {
                var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.AccountId == accountId && !a.IsDeleted);

                account.Balance -= amount;
                _context.BankAccounts.Update(account);
                await _context.SaveChangesAsync();

                await RecordTransactionDetailsAsync(account,null,amount,0,0,remarks,false,true);
            }
            catch { }
            return false;
        }

        public async Task<AccountStatement> GetAccountStatement(int accountId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var account = await _context.BankAccounts.Where(item => item.AccountId == accountId).FirstOrDefaultAsync();
                var viewModel = new AccountStatement
                {
                    AccountId = accountId,
                    AccountName = account.BankAccountUserName, 
                    AccountNumber = account.AccountNumber, 
                    FromDate = fromDate,
                    ToDate = toDate,
                    Transactions = await _context.TransactionSummary.Where(item => item.AccountId == accountId && fromDate <= item.TransactionDate && item.TransactionDate <= toDate).Include(item => item.TransactionDetail).ToListAsync()
                };
                return viewModel;
            }
            catch { }
            return new AccountStatement();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.TransactionDetails.Where(item => item.TransactionId == transactionId).Include(item => item.SenderAccount).Include(item => item.ReceiverAccount).FirstOrDefaultAsync() ?? new Transaction();
        }
    }
}
