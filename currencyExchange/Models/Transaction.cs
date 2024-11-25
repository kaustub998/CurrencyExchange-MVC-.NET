using Microsoft.EntityFrameworkCore;

namespace currencyExchange.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? ReceiverAccountId { get; set; }
        [Precision(18,2)]
        public decimal? SenderAmount { get; set; }
        [Precision(18, 2)]
        public decimal? ReceiverAmount { get; set; }
        [Precision(18, 6)]
        public decimal? ExchangeRate { get; set; }
        public string? SenderCurrency { get; set; }
        public string? ReceiverCurrency { get; set; }
        public string? TransactionStatus { get; set; }
        public string? CalculationFormula { get; set; }
        public string? Remarks { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public virtual BankAccount? SenderAccount { get; set; }
        public virtual BankAccount? ReceiverAccount { get; set; }
    }
    public class TransactionSummary
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        [Precision(18, 2)]
        public decimal? Amount { get; set; }
        [Precision(18, 2)]
        public decimal? OldBalance { get; set; }
        public bool Credited { get; set; }
        public bool Debited { get; set; }
        public bool IsForeignTransaction { get; set; }
        public int? TransactionDetailId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? TransactionStatus { get; set; }
        public string? Remarks { get; set; }

        public virtual Transaction? TransactionDetail { get; set; }
    }

    public class AccountStatement
    {
        public int AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<TransactionSummary> Transactions { get; set; }
        public Transaction SelectedTransaction { get; set; }
    }

}
