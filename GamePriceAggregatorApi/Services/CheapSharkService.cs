using System.Text.Json;
using System.Text.RegularExpressions;
using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;

namespace GamePriceAggregatorApi.Services;

public class CheapSharkService : IGameService
{
    private readonly HttpClient _httpClient;

    public CheapSharkService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string exactTitle)
    {
        if (string.IsNullOrWhiteSpace(exactTitle))
            return Enumerable.Empty<GameResult>();

        var searchUrl = $"https://www.cheapshark.com/api/1.0/deals?" +
                        $"title={Uri.EscapeDataString(exactTitle)}" +
                        //$"&exact=1" +                    
                        $"&storeID=7,25" +               
                        $"&upperPrice=60";
        Console.WriteLine(searchUrl);
        try
        {
            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var results = new List<GameResult>();

            foreach (var deal in doc.RootElement.EnumerateArray())
            {
                var title = deal.GetProperty("title").GetString() ?? "";
                var storeId = deal.GetProperty("storeID").GetString() ?? "";
                var price = deal.GetProperty("salePrice").GetString() ?? "0";
                var thumb = deal.GetProperty("thumb").GetString() ?? "";

                var storeName = storeId == "7" ? "GOG" : "Epic Games";

                results.Add(new GameResult
                {
                    Title = title,
                    Price = price,
                    Store = storeName,
                    ThumbnailUrl = thumb,
                    ExternalUrl = $"https://www.cheapshark.com/redirect?dealID={deal.GetProperty("dealID").GetString()}"
                });
            }

            return results
                .GroupBy(r => r.Store)                    
                .Select(g => g.OrderBy(r => decimal.TryParse(r.Price, out var p) ? p : 999).First())
                .OrderBy(r => decimal.TryParse(r.Price, out var p) ? p : 999)
                .ToList();
        }
        catch
        {
            return Enumerable.Empty<GameResult>();
        }
    }

}