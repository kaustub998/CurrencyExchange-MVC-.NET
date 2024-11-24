using currencyExchange.Models;

namespace currencyExchange.Services.ForexService
{
    public interface IForexService
    {
        Task<List<ForexRate>> GetForexRatesAsync();
    }
}
