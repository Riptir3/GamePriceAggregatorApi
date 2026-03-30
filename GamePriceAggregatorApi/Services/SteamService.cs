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
        var url = $"api/storesearch?term={Uri.EscapeDataString(searchTerm)}&l=hungarian&cc=HU";

        var response = await _httpClient.GetAsync(url);
        Console.WriteLine(response);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<GameResult>();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var results = new List<GameResult>();

        if (doc.RootElement.TryGetProperty("items", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "";
                if (name.Contains("DLC", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Expansion", StringComparison.OrdinalIgnoreCase)) continue;

                var id = item.GetProperty("id").GetInt32();
                string priceLabel = "0.00";

                if (item.TryGetProperty("price", out var priceInfo))
                {
                    priceLabel = (priceInfo.GetProperty("final").GetInt32() / 100.0).ToString("0.00").Replace(",", ".");
                }

                results.Add(new GameResult
                {
                    Title = name,
                    Price = priceLabel,
                    Store = "Steam",
                    ThumbnailUrl = item.GetProperty("tiny_image").GetString() ?? "",
                    ExternalUrl = $"https://store.steampowered.com/app/{id}"
                });
            }
        }
        return results;
    }
}