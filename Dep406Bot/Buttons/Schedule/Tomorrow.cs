using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using ServerServices.LectorSchedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ThirdParty.Json.LitJson;

namespace Dep406Bot.Buttons.Schedule
{
    [Description("Tomorrow")]
    internal class Tomorrow(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            string group = "";

            var scheduleBT = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Сегодня", "Today"),
                        InlineKeyboardButton.WithCallbackData("Завтра", "Tomorrow"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Неделя", "Week"),
                        InlineKeyboardButton.WithCallbackData("Следующая Неделя", "NextWeek"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("На главную", "MainMenu"),
                    }
                }
            );


            try
            {
                group = await _db.getGroup(update.CallbackQuery.Message.Chat.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + $"/schedule/students/{group}" +
                $"/today?now={DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")}";

            try
            {
                HttpResponseMessage responseBody = await _api.GetAsync(schApiLink);

                ScheduleStudentLesson[] data = await responseBody.Content.ReadFromJsonAsync<ScheduleStudentLesson[]>();

                string awnser = $"<pre>📅{data[0].Start.ToString("dddd", new CultureInfo("ru-RU"))}     {data[0].Start.ToString("dd MMMM", new CultureInfo("ru-RU"))}</pre>\n\n";

                foreach (var leson in data)
                {
                    awnser += $"\t                     <b>{leson.Start.ToString("hh:mm")}-{leson.End.ToString("hh:mm")}</b>\n" +
                              $" {leson.Name.ToString()}\n" +
                              $"<i>{leson.Lectors.Values.Aggregate((current, next) => current + "\n" + next)}</i>\n" +
                              $"\t         {leson.Types.Keys
                                              .Select(i => i.ToString())
                                              .Aggregate((current, next) => current + "\n" + next)}" +
                              $"\t                      <b>{leson.Rooms.Values
                                                                        .Select(i => i.ToString())
                                                                        .Aggregate((current, next) => current + "\n" + next)}</b>\n\n";
                }

                await client.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    awnser,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                await client.EditMessageReplyMarkup(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    replyMarkup: scheduleBT);
                return;

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

            await client.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                "расписание не найдено");
            await client.EditMessageReplyMarkup(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                replyMarkup: scheduleBT);
            return;

        }
    }
}
