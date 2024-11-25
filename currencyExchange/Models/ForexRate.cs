namespace currencyExchange.Models
{
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
