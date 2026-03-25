using System.Text;
using System.Text.Json;
using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;

namespace GamePriceAggregatorApi.Services;

public class EpicService : IGameService
{
    private readonly HttpClient _httpClient;

    public EpicService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string searchTerm)
    {
        var url = "https://graphql.epicgames.com/graphql";

        var query = new
        {
            query = @"query searchStoreQuery($keywords: String) {
                Catalog {
                    searchStore(keywords: $keywords) {
                        elements {
                            title
                            price {
                                totalPrice {
                                    fmtPrice {
                                        intermediatePrice
                                    }
                                }
                            }
                            catalogNs { mappings(pageType: ""productHome"") { pageSlug } }
                        }
                    }
                }
            }",
            variables = new { keywords = searchTerm }
        };

        var jsonQuery = JsonSerializer.Serialize(query);
        var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<GameResult>();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            var results = new List<GameResult>();
            var elements = doc.RootElement
                .GetProperty("data").GetProperty("Catalog").GetProperty("searchStore").GetProperty("elements");

            foreach (var element in elements.EnumerateArray())
            {
                var title = element.GetProperty("title").GetString() ?? "";
                var priceStr = element.GetProperty("price").GetProperty("totalPrice")
                                      .GetProperty("fmtPrice").GetProperty("intermediatePrice").GetString() ?? "0";

                results.Add(new GameResult
                {
                    Title = title,
                    Price = priceStr.Replace(" ", " "),
                    Store = "Epic Games",
                    ThumbnailUrl = "https://store.epicgames.com/p/" 
                });
            }
            return results;
        }
        catch
        {
            return Enumerable.Empty<GameResult>();
        }
    }
}