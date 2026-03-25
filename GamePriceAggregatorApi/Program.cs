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

            //builder.Services.AddScoped<IGameService, SteamService>();
            builder.Services.AddScoped<IGameService, EnebaService>();
            //builder.Services.AddScoped<IGameService, CheapSharkService>();

            builder.Services.AddHttpClient<EnebaService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(50);
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
