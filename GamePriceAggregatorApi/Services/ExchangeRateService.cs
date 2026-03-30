using System.Text.Json;

namespace GamePriceAggregatorApi.Services;

public class ExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private decimal _eurToHuf = 400m;
    private decimal _usdToHuf = 370m;
    private DateTime _lastUpdate = DateTime.MinValue;

    public ExchangeRateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(decimal EurToHuf, decimal UsdToHuf)> GetRatesAsync()
    {
        if ((DateTime.Now - _lastUpdate).TotalHours > 1)
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync("https://api.frankfurter.app/latest?from=EUR&to=HUF,USD");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var rates = doc.RootElement.GetProperty("rates");

                    _eurToHuf = rates.GetProperty("HUF").GetDecimal();
                    var eurToUsd = rates.GetProperty("USD").GetDecimal();

                    _usdToHuf = _eurToHuf / eurToUsd;
                    _lastUpdate = DateTime.Now;
                }
            }
            catch { }
        }

        return (_eurToHuf, _usdToHuf);
    }
}
