using Dep406Bot.CustomAttribute;
using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.model;
using Dep406Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons.Chooses
{
    [BotCommandsAtribute(["1faculty", "2faculty", "3faculty", "4faculty", "5faculty", "6faculty", "7faculty", "8faculty", "9faculty", "10faculty",
                            "11faculty", "12faculty", "13faculty", "14faculty"])]

    internal class ChooseFacultyBT(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            EntityChatHistory chat_history = new();

            try
            {
                var facultINT = update.CallbackQuery.Data.Replace("faculty", "");

                await _db.setFaculty(update.CallbackQuery.Message.Chat.Id, Convert.ToInt16(facultINT.ToString()));

                chat_history = await _db.getChatHistory(update.CallbackQuery.Message.Chat.Id);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            string? schApiLink = Environment.GetEnvironmentVariable("ScheduleAPILink") + "/index/students/names";

            try
            {
                string responseBody = await _api.GetAPIResponseAsync(schApiLink);

                // Десериализация JSON в объект
                var data = await _api.DeserializeAPIResponse<string[]>(responseBody);

                data = GroupChecker(data, chat_history);

                string str = data
                            .ToArray()
                            .Aggregate((current, next) => current + "\n" + next);
                Console.WriteLine("Ответ от API: " + str);

                // Заполнение кнопок 3 в строчку и обработку остаточных кнопок
                for(int l = 0; l < data.Length;) 
                {
                    var bt = new InlineKeyboardButton[] { };
                    if(data.Length - l >= 3) {
                        bt = (new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData(data[l].ToString(), $"ChooseGroup/{data[l].ToString()}"),
                                    InlineKeyboardButton.WithCallbackData(data[l+1].ToString(), $"ChooseGroup/{data[l+1].ToString()}"),
                                    InlineKeyboardButton.WithCallbackData(data[l+2].ToString(), $"ChooseGroup/{data[l+2].ToString()}") 
                                }
                                );
                        buttons.Add(bt);

                        l += 3;
                    }
                    else if (data.Length - l == 2)
                    {
                        bt =
                            (
                            new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData(data[l].ToString(), $"ChooseGroup/{data[l].ToString()}"),
                                    InlineKeyboardButton.WithCallbackData(data[l+1].ToString(), $"ChooseGroup/{data[l+1].ToString()}"),
                                }
                            );
                        buttons.Add(bt);

                        l += 2;
                    }
                    else
                    {
                        bt =
                            (
                            new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData(data[l].ToString(), $"ChooseGroup/{data[l].ToString()}"),
                                }
                            );
                        buttons.Add(bt);

                        l++;
                    }

                }

                var chooseGroupBT = new InlineKeyboardMarkup(buttons);

                await client.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Выбери группу:");
                await client.EditMessageReplyMarkup(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    replyMarkup: chooseGroupBT);
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
                "Выбери группу:");
            await client.EditMessageReplyMarkup(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId);
            return;
        }

        private string[] GroupChecker(string[] allGroups, EntityChatHistory chatHistory) 
        {

            List<string> groups = new();

            for(int i = 0; i < allGroups.Length; i++) 
            {
                var split = allGroups[i].ToString()
                                        .Split("-");

                if (
                    split[0][1].ToString() == chatHistory.Faculty.ToString() &&
                    split[1][0].ToString() == chatHistory.Year.ToString() &&
                    IndificatorChecker(split[1].Substring(3), chatHistory)
                    ) 
                {
                    groups.Add(allGroups[i]);
                }
            }

            return groups.ToArray();
        }
        /// <summary>
        /// 0 - если у пользователя нет регистрации<br/>
        /// 1 - студент бакалавриата => есть название группы<br/>
        /// 2 - студент специалитета
        /// 3 - студент магистратуры
        /// 4 - студет аспирантуры
        /// 5 - преподаватель
        /// Можно будет выбрать если преподаватель еще не получил акредитацию<br/>
        /// 6 - зарегистрированный преподаватель (с подтверждением)
        /// зарегистрированый преподаватель сможет изменять кабинеты для расписания, назначаить новые занятия и отменять их<br/>
        /// сможет добавлять заметки и дз к расписанию<br/>
        /// </summary>
        private bool IndificatorChecker(string temp, EntityChatHistory chatHistory) 
        {
            if (temp == "Б" && chatHistory.Indificator == 1) return true;
            else if (temp == "БВ" && chatHistory.Indificator == 1) return true;
            else if (temp == "С" && chatHistory.Indificator == 2) return true;
            else if (temp == "СВ" && chatHistory.Indificator == 2) return true;
            else if (temp == "М" && chatHistory.Indificator == 3) return true;
            else if (temp == "А" && chatHistory.Indificator == 4) return true;
            else return false;
        }


    }
}
