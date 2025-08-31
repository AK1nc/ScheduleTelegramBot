using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using MathCore.Net.Http.Html;
using Microsoft.VisualBasic;
using ScheduleTelegramBot.ScheduleAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;


namespace Dep406Bot.Commands
{
    [Description("/преподаватели")]
    internal class AllLectorsCommand(IChatHistory _db, IHttpAPIClient APIclient) : IBotCommand
    {

        public IHttpAPIClient APIclient;

        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }


        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + "/index/lectors/name";
           
            try
            {
                string responseBody = await APIclient.GetAPIResponseAsync(schApiLink);

                // Десериализация JSON в объект
                var data = await APIclient.DeserializeAPIResponse<string[]>(responseBody);

                string str = data
                            .Take(10)
                            .ToArray()
                            .Aggregate((current, next) => current + "\n" + next);
                Console.WriteLine("Ответ от API: " + str);
                await client.SendMessage(
                        update.Message.Chat.Id,
                        str
                        );

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nОшибка HTTP-запроса: " + e.Message);
            }
            catch (JsonException e)
            {
                Console.WriteLine("\nОшибка при десериализации JSON: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nПроизошла неизвестная ошибка: " + e.Message);
            }
            
        }
    }
}
