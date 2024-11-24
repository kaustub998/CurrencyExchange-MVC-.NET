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

    public class ForexRate
    {
        public Currency? Currency { get; set; }
        public decimal? Buy { get; set; }
        public decimal? Sell { get; set; }
    }

    public class Rate
    {
        public Currency? Currency { get; set; }
        public string? Buy { get; set; }
        public string? Sell { get; set; }
    }

    public class Currency
    {
        public string Iso3 { get; set; }
        public string Name { get; set; }
        public int Unit { get; set; }
    }

    public class ForexApiResponse
    {
        public Data Data { get; set; }
        public Status Status { get; set; }
    }

    public class Data
    {
        public List<Payload> Payload { get; set; }
    }

    public class Payload
    {
        public string Date { get; set; }
        public List<Rate> Rates { get; set; }
    }

    public class Status
    {
        public int Code { get; set; }
    }


}
