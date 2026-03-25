using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using HtmlAgilityPack;
using System.Xml;

namespace GamePriceAggregatorApi.Services;

public class EnebaService : IGameService
{
    private readonly HttpClient _httpClient;

    public EnebaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GameResult>> SearchGamesAsync(string searchTerm) => null!;
}