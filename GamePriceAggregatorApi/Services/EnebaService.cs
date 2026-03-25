using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using System.Text.Json;
using System.Net;

namespace GamePriceAggregatorApi.Services;

public class EnebaService : IGameService
{
    private readonly HttpClient _httpClient;

    public EnebaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(45);
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<GameResult>();

        var results = new List<GameResult>();
        var searchUrl = $"https://www.eneba.com/search?q={Uri.EscapeDataString(name)}";

        try
        {
            var flarePayload = new
            {
                cmd = "request.get",
                url = searchUrl,
                maxTimeout = 40000,
                returnOnlyCookies = false
            };

            var flareResponse = await _httpClient.PostAsJsonAsync("http://flaresolverr:8191/v1", flarePayload);

            if (!flareResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"FlareSolverr hiba: {flareResponse.StatusCode}");
                return results;
            }

            var flareJson = await flareResponse.Content.ReadFromJsonAsync<JsonElement>();
            var html = flareJson.GetProperty("solution").GetProperty("response").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(html))
                return results;

            // HTML feldolgozás
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var productNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-card')]")
                               ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'MuiCard-root')]")
                               ?? doc.DocumentNode.SelectNodes("//a[contains(@href, '/product/')]/ancestor::div[1]");

            if (productNodes == null || productNodes.Count == 0)
                return results;

            foreach (var node in productNodes.Take(8))
            {
                var titleNode = node.SelectSingleNode(".//h3 | .//span[contains(@class, 'title')]");
                var priceNode = node.SelectSingleNode(".//span[contains(@class, 'price')] | .//div[contains(@class, 'price')]");
                var linkNode = node.SelectSingleNode(".//a[contains(@href, '/product/') or contains(@href, '/gog-') or contains(@href, '/steam-')]");
                var imgNode = node.SelectSingleNode(".//img");

                var title = titleNode?.InnerText?.Trim() ?? "";
                var price = priceNode?.InnerText?.Trim() ?? "";
                var link = linkNode?.GetAttributeValue("href", "") ?? "";
                var thumb = imgNode?.GetAttributeValue("src", "") ?? imgNode?.GetAttributeValue("data-src", "");

                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(price))
                {
                    results.Add(new GameResult
                    {
                        Title = WebUtility.HtmlDecode(title),
                        Price = WebUtility.HtmlDecode(price.Replace(" ", "")),
                        Store = "Eneba",
                        ThumbnailUrl = thumb.StartsWith("http") || thumb.StartsWith("//")
                            ? thumb
                            : "https://www.eneba.com" + thumb,
                        ExternalUrl = link.StartsWith("http")
                            ? link
                            : "https://www.eneba.com" + link
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EnebaService hiba: {ex.Message}");
        }

        return results;
    }
}