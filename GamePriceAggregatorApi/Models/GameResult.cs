namespace GamePriceAggregatorApi.Models
{
    public class GameResult
    {
        public string Title { get; set; } = string.Empty;
        public string Price { get; set; } = "N/A";
        public string Store { get; set; } = string.Empty;
        public string? AppId { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
