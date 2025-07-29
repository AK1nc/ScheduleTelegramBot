using Dep406Bot.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Commands
{
    [Description("/IsRoomFree")]
    internal class IsRoomFree : IBotCommand
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty, Encoding.UTF8);
            
            var mes = update.Message.Text.Split(" ");
            
            var schApiLink = $"{Environment.GetEnvironmentVariable("ScheduleAPILink")}/schedule/rooms/name/{mes[1]}/today";
            if (mes.Length == 3) 
            {
                //var date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff");

                // создаем нынешнее время в которое будем парсить то что передал пользователь
                // делаем это что бы если пользователь напишет только время (тогда автоматом смотрится нынешний день

                queryString["now"] = mes[2]; 
                schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + "/schedule/rooms/name/" + mes[1] + "/today?" + mes[2]; //$"{queryString}"
            }

            using HttpClient APIlient = new();
            try
            {
                HttpResponseMessage response = await APIlient.GetAsync(schApiLink, cancellationToken).ConfigureAwait(false);

                var responseBody = await response
                    .EnsureSuccessStatusCode()
                    .Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                Console.WriteLine("Ответ от API: " + responseBody);

                // Десериализация JSON в объект
                var data = JsonSerializer.Deserialize<string[]>(responseBody);

                        
                await client.SendMessage(
                        update.Message.Chat.Id,
                        "test"
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
