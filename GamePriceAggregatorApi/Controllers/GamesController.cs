using GamePriceAggregatorApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamePriceAggregatorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Search for a valid game...");

        var results = await _gameService.SearchGamesAsync(name);
        return Ok(results);
    }
}