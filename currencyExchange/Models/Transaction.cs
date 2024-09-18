namespace currencyExchange.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int SenderAccountId { get; set; }
        public int ReceiverAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public virtual BankAccount? SenderAccount { get; set; }
        public virtual BankAccount? ReceiverAccount { get; set; }
    }
}
