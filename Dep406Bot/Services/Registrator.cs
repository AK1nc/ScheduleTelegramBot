using Dep406Bot.model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dep406.ScheduleAPI;
using System.Runtime.CompilerServices;
using Dep406Bot.Data;
using MongoDB.Driver.Core.Configuration;
using Dep406Bot.Data.Interface;

namespace Dep406Bot.Services
{
    internal static class Registrator
    {

        public static IServiceCollection AddDep406TelegramBotServices(this IServiceCollection services)
            => services
            .AddHttpClient()
            .AddAPIHttpClient()
            .AddHostedService<BotHost>()
            .AddDbChatContext();


        //public static IServiceCollection AddBotClient(this IServiceCollection srv) => srv.


        public static IServiceCollection AddAPIHttpClient(this IServiceCollection srv) =>
           srv
             .AddSingleton<IHttpAPIClient, HttpAPIClientService>();

        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        // options.UseSqlServer(connectionString));

        public static IServiceCollection AddDbChatContext(this IServiceCollection srv) => srv
            .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=chatHistory;Trusted_Connection=True;MultipleActiveResultSets=true"))
            .AddTransient<IChatHistory, ChatHistoryDB>();


    }
}
