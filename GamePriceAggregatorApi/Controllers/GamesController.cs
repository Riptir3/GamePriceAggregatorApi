using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamePriceAggregatorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IEnumerable<IGameService> _services;

    public GamesController(IEnumerable<IGameService> services)
    {
        _services = services;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        Console.WriteLine($"Keresés indítása: {name}. Szervizek száma: {_services.Count()}");
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Search a valid game....");

        var searchTasks = _services.Select(s => s.SearchGamesAsync(name));

        var resultsArray = await Task.WhenAll(searchTasks);

        var finalResults = resultsArray.SelectMany(r => r)
                                       .OrderBy(g => g.Price); 

        return Ok(finalResults);
    }
}