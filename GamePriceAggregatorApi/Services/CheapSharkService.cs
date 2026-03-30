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

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string searchTerm)
    {
        var url = $"deals?title={Uri.EscapeDataString(searchTerm)}&storeID=7,25&nonSpecific=1";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<GameResult>();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var results = new List<GameResult>();

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var title = item.GetProperty("title").GetString() ?? "";

            if (title.Contains("Add-On", StringComparison.OrdinalIgnoreCase)) continue;

            results.Add(new GameResult
            {
                Title = title,
                Price = item.GetProperty("salePrice").GetString() ?? "0.00",
                Store = item.GetProperty("storeID").GetString() == "7" ? "GOG" : "Epic Games",
                ThumbnailUrl = item.GetProperty("thumb").GetString() ?? "",
                ExternalUrl = $"https://www.cheapshark.com/redirect?dealID={item.GetProperty("dealID").GetString()}"
            });
        }
        return results;
    }

}