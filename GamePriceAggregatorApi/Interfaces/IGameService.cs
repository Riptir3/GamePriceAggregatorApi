using GamePriceAggregatorApi.Models;

namespace GamePriceAggregatorApi.Interfaces
{
    public interface IGameService
    {
        Task<IEnumerable<GameResult>> SearchGamesAsync(string searchTerm);
    }
}
