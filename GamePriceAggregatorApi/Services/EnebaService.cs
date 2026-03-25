using HtmlAgilityPack;
using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using System.Net;

namespace GamePriceAggregatorApi.Services;

public class EnebaService : IGameService
{
    private readonly HttpClient _httpClient;

    public EnebaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string name)
    {
        var results = new List<GameResult>();
        var searchUrl = $"https://www.eneba.com/hu/store/all?text={Uri.EscapeDataString(name)}";

        try
        {
            var html = await _httpClient.GetStringAsync(searchUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'pFaGHa')]");

            if (nodes != null)
            {
                foreach (var node in nodes.Take(5))
                {
                    var title = node.SelectSingleNode(".//span[contains(@class, 'YLosEL')]")?.InnerText;

                    var price = node.SelectSingleNode(".//span[contains(@class, 'L5ErLT')]")?.InnerText;

                    var link = node.SelectSingleNode(".//a[contains(@class, 'oSVLlh')]")?.GetAttributeValue("href", "");

                    var thumb = node.SelectSingleNode(".//img[contains(@class, 'LBwiWP')]")?.GetAttributeValue("src", "");

                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(price))
                    {
                        results.Add(new GameResult
                        {
                            Title = WebUtility.HtmlDecode(title).Trim(),
                            Price = WebUtility.HtmlDecode(price).Replace("&nbsp;", " ").Trim(),
                            Store = "Eneba",
                            ThumbnailUrl = thumb,
                            ExternalUrl = "https://www.eneba.com" + link
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eneba hiba: {ex.Message}");
        }

        return results;
    }
}