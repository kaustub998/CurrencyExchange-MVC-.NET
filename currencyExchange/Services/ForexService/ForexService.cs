using currencyExchange.Models;
using System.Text.Json;
using XAct;

namespace currencyExchange.Services.ForexService
{
    public class ForexService : IForexService
    {
        private readonly HttpClient _httpClient;

        public ForexService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ForexRate>> GetForexRatesAsync()
        {
            var fromDate = DateTime.Now.ToString("yyyy-MM-dd");
            var toDate = DateTime.Now.ToString("yyyy-MM-dd");

            var response = await _httpClient.GetAsync($"https://www.nrb.org.np/api/forex/v1/rates?page=1&per_page=5&from={fromDate}&to={toDate}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching forex rates. Status Code: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var forexApiResponse = JsonSerializer.Deserialize<ForexApiResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (forexApiResponse?.Data?.Payload == null)
            {
                return new List<ForexRate>(); 
            }

            var rates = new List<ForexRate>();

            foreach (var payload in forexApiResponse.Data.Payload)
            {
                if (payload?.Rates != null)
                {
                    foreach (var rate in payload.Rates)
                    {
                       var buyRate = Convert.ToDecimal(rate.Buy); 
                        var sellRate = Convert.ToDecimal(rate.Sell); 

                        rates.Add(new ForexRate
                        {
                            Currency = rate.Currency,
                            Buy = buyRate,
                            Sell = sellRate
                        });
                    }
                }
            }

            return rates;
        }
    }
}
