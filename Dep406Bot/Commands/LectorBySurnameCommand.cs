using Dep406Bot.Interface;
using ScheduleTelegramBot.ScheduleAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Commands
{
    [Description("/преподаватель")]
    internal class LectorBySurnameCommand : IBotCommand
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + "/index/lectors/";
            using (HttpClient APIlient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await APIlient.GetAsync(schApiLink);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Ответ от API: " + responseBody);
                    // Десериализация JSON в объект
                    var data = JsonSerializer.Deserialize<LectorInfo[]>(responseBody);

                    string str = data
                                .Select(s => s.Name)
                                .Take(10)
                                .ToArray()
                                .Aggregate((current, next) => current + " " + next);
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
}
