using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using Ical.Net.DataTypes;
using ServerServices.LectorSchedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons.Schedule
{
    [Description("NextWeek")]
    internal class NextWeek(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        Dictionary<DateTime, ScheduleStudentLesson[]> weekData = new();

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

            var culture = CultureInfo.CurrentCulture;
            var weekOffset = culture.DateTimeFormat.FirstDayOfWeek - DateTime.Now.DayOfWeek;
            var startOfWeek = DateTime.Today.AddDays(weekOffset);



            try
            {
                for (int i = 0; i < 7; i++)
                {
                    string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + $"/schedule/students/{group}" +
                                                                            $"/today?now={startOfWeek.AddDays(7+i).ToString("yyyy-MM-dd")}";

                    HttpResponseMessage responseBody = await _api.GetAsync(schApiLink);

                    ScheduleStudentLesson[] tempData = await responseBody.Content.ReadFromJsonAsync<ScheduleStudentLesson[]>();

                    weekData.Add(startOfWeek.AddDays(i), tempData);
                }

                string awnser = "";

                foreach (var data in weekData)
                {
                    if (data.Value.Count() != 0)
                    {
                        awnser += $"<pre>📅{data.Value[0].Start.ToString("dddd", new CultureInfo("ru-RU"))}   " +
                            $"{data.Value[0].Start.ToString("dd MMMM", new CultureInfo("ru-RU"))}</pre>\n\n";

                        foreach (var leson in data.Value)
                        {
                            if (leson.Lectors.Count() != 0 && leson.Rooms.Count() != 0 && leson.Types.Count() != 0)
                            {
                                awnser += $"\t                                       <b>{leson.Start.ToString("HH:mm")}-{leson.End.ToString("HH:mm")}</b>\n" +
                                          $" {leson.Name.ToString()}\n" +
                                          $"<i>{leson.Lectors.Values.Aggregate((current, next) => current + "\n" + next)}</i>\n" +
                                          $"\t                          {leson.Types.Keys
                                                                                    .Select(i => i.ToString())
                                                                                    .Aggregate((current, next) => current + "\n" + next)}" +
                                          $"\t                              <b>{leson.Rooms.Values
                                                                                              .Select(i => i.ToString())
                                                                                              .Aggregate((current, next) => current + "\n" + next)}</b>\n\n";
                            }
                            else
                            {
                                awnser += $"\t                                       <b>{leson.Start.ToString("HH:mm")}-{leson.End.ToString("HH:mm")}</b>\n\n" +
                                          $" {leson.Name.ToString()}\n\n";
                            }
                        }
                    }
                    else
                    {
                        awnser += awnser = $"<pre>📅{data.Key.ToString("dddd", new CultureInfo("ru-RU"))}   " +
                            $"{data.Key.ToString("dd MMMM", new CultureInfo("ru-RU"))}</pre>\n\n" +
                            $"                             Нет занятий\n";
                    }

                }

                awnser += "\n-------------------------------------------------------------------------------------";

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
