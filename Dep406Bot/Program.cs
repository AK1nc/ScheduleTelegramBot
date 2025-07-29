using Dep406Bot.model;
using Microsoft.Extensions.Configuration;


namespace Dep406Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>() 
                .Build();

            Environment.SetEnvironmentVariable("ScheduleAPILink", "http://localhost:8080");

            string Token = configuration["TelegramBot:ServiceApiKey"];

            BotHost tgBOT = new BotHost(Token);


            await tgBOT.BotStart();

        }
    }
}
