using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using ServerServices.LectorSchedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons.Schedule
{
    [Description("Schedule")]
    internal class Schedule(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            string group = "";

            try
            {
                group = await _db.getGroup(update.CallbackQuery.Message.Chat.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + $"/schedule/students/{group}/today";

            try
            {
                string responseBody = await _api.GetAPIResponseAsync(schApiLink);

                // Десериализация JSON в объект
                var data = await _api.DeserializeAPIResponse<ScheduleStudentLesson>(responseBody);

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


            await client.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                "Расписание");
            await client.EditMessageReplyMarkup(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                replyMarkup: scheduleBT);
            return;

        }
    }
}
