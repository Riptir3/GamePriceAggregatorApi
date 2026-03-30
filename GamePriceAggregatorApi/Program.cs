using GamePriceAggregatorApi.Interfaces;
using GamePriceAggregatorApi.Services;
using Scalar.AspNetCore;

namespace GamePriceAggregatorApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient();

            builder.Services.AddHttpClient<SteamService>(client => {
                client.BaseAddress = new Uri("https://store.steampowered.com/");
            });

            builder.Services.AddHttpClient<CheapSharkService>(client => {
                client.BaseAddress = new Uri("https://www.cheapshark.com/api/1.0/");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0...");
            });

            builder.Services.AddScoped<IGameService>(sp => sp.GetRequiredService<SteamService>());
            builder.Services.AddScoped<IGameService>(sp => sp.GetRequiredService<CheapSharkService>());

            builder.Services.AddScoped<GameAggregatorService>();
            builder.Services.AddSingleton<ExchangeRateService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
