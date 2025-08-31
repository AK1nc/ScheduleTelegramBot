using Dep406.ScheduleAPI;
using Dep406Bot.Data;
using Dep406Bot.Data.Interface;
using Dep406Bot.model;
using Dep406Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScheduleTelegramBot.ScheduleAPI;
using System;
using System.Globalization;


namespace Dep406Bot
{
    internal class Program
    {

        private static string baseURL = "https://localhost:8080/";

        static async Task Main(string[] args)
        {
            

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>();
            var _host = Host
                .CreateDefaultBuilder(args)

                .ConfigureAppConfiguration
                (
                cfg => cfg = configuration
                )

                .ConfigureServices
                (
                src => src
                .AddDep406TelegramBotServices()
                .AddMAISchedule(baseURL)
                );

            using var app = _host.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            var app_config = app.Services.GetRequiredService<IConfiguration>();

            var APIservice = app.Services.GetService<IHttpAPIClient>();
            var BDservice = app.Services.GetService<IChatHistory>();

            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            Environment.SetEnvironmentVariable("ScheduleAPILink", "http://localhost:8080");

            string Token = app_config["TelegramBot:ServiceApiKey"];

            BotHost tgbot = new(APIservice, BDservice, "8166934425:AAEJ1VpRIVuqEfmKX5UNgE9NxnIapXWStF4");

            await tgbot.StartAsync(CancellationToken.None);

            await _host.Build().RunAsync();


        }
    }
}
