using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Models;
using GamePriceAggregatorApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamePriceAggregatorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GameAggregatorService _gameAggregatorService;

    public GamesController(GameAggregatorService gameAggregatorService)
    {
        _gameAggregatorService = gameAggregatorService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        Console.WriteLine($"Search start for: {name}.");
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Search a valid game....");

        var finalResults = await _gameAggregatorService.GetCombinedResultsAsync(name);
        return Ok(finalResults);
    }
}