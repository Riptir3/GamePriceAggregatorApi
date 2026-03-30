using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using System.Globalization; // Ezt hozzáadtam a tizedespont kezeléshez

namespace GamePriceAggregatorApi.Services;

public class GameAggregatorService
{
    private readonly IEnumerable<IGameService> _gameServices;
    private readonly ExchangeRateService _exchangeRateService;

    public GameAggregatorService(IEnumerable<IGameService> gameServices, ExchangeRateService exchangeRateService)
    {
        _gameServices = gameServices;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<IEnumerable<GameResult>> GetCombinedResultsAsync(string searchTerm)
    {
        var (eurRate, usdRate) = await _exchangeRateService.GetRatesAsync();
        var tasks = _gameServices.Select(s => s.SearchGamesAsync(searchTerm));
        var resultsArray = await Task.WhenAll(tasks);

        var allResults = new List<GameResult>();

        foreach (var serviceResults in resultsArray)
        {
            foreach (var game in serviceResults)
            {
                if (string.IsNullOrWhiteSpace(game.Price) ||
                    game.Price.Contains("Free", StringComparison.OrdinalIgnoreCase) ||
                    game.Price == "0" || game.Price == "0.00")
                {
                    game.Price = "Free / No price";
                    allResults.Add(game);
                    continue;
                }

                string cleanPrice = game.Price.Replace(",", ".");
                if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericPrice))
                {
                    decimal finalHuf = 0;

                    if (game.Store == "Steam")
                    {
                        finalHuf = numericPrice * eurRate;
                    }
                    else if (game.Store == "GOG" || game.Store == "Epic Games")
                    {
                        finalHuf = numericPrice * usdRate;
                    }

                    game.Price = finalHuf > 0 ? $"{Math.Round(finalHuf, 0):N0} Ft" : "Free";
                    allResults.Add(game);
                }
            }
        }

        return allResults.OrderBy(GetNumericPrice).ToList();
    }

    private decimal GetNumericPrice(GameResult game)
    {
        if (game.Price == "Free") return 0;
        var onlyDigits = new string(game.Price.Where(char.IsDigit).ToArray());

        if (!string.IsNullOrEmpty(onlyDigits))
        {
            return decimal.Parse(onlyDigits);
        }

        return 999999; 
    }
}