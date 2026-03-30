using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;

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
                if (game.Price.Contains("Free", StringComparison.OrdinalIgnoreCase) || game.Price == "0" || game.Price == "0.00")
                {
                    game.Price = "Free";
                    allResults.Add(game);
                    continue; 
                }

                if (decimal.TryParse(game.Price.Replace(".", ","), out var numericPrice))
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
                    game.Price = finalHuf > 0 ? $"{finalHuf:N0} Ft" : "Free";
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
        return decimal.TryParse(onlyDigits, out var res) ? res : 999999;
    }
}