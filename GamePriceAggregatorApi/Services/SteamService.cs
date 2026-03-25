using System.Text.Json;
using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;

namespace GamePriceAggregatorApi.Services;

public class SteamService : IGameService
{
    private readonly HttpClient _httpClient;

    public SteamService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string searchTerm)
    {
        var url = $"https://store.steampowered.com/api/storesearch/?term={searchTerm}&l=hungarian&cc=HU";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<GameResult>();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var results = new List<GameResult>();

        if (doc.RootElement.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                var title = item.GetProperty("name").GetString() ?? "Ismeretlen";
                var id = item.GetProperty("id").GetInt32();

                string priceLabel = "Free / No price";
                if (item.TryGetProperty("price", out var priceInfo))
                {
                    var finalPrice = priceInfo.GetProperty("final").GetInt32() / 100.0;
                    priceLabel = $"{finalPrice:N0}";
                }

                results.Add(new GameResult
                {
                    Title = title,
                    Price = priceLabel,
                    Store = "Steam",
                    ThumbnailUrl = $"https://store.steampowered.com/app/{id}"
                });
            }
        }

        return results;
    }
}