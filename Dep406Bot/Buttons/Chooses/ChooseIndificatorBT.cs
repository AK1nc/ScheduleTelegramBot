using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons.Chooses
{
    [Description("ShooseIndificator")]
    internal class ChooseIndificatorBT(IChatHistory _db) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var chooseIndBT = new InlineKeyboardMarkup
                (
                    new List<InlineKeyboardButton[]>() 
                    {
                          new InlineKeyboardButton[] 
                          {
                              InlineKeyboardButton.WithCallbackData("Студент бакалавриата", "BachelorStudent"),
                              InlineKeyboardButton.WithCallbackData("Студент специалитета", "SpecialistStudent"),
                          },
                          new InlineKeyboardButton[]
                          {
                              InlineKeyboardButton.WithCallbackData("Струдент магистратуры", "MasterStudent"),
                              InlineKeyboardButton.WithCallbackData("Студент аспирантуры", "PostgraduateStudent"),
                          },
                          new InlineKeyboardButton[]
                          {
                              InlineKeyboardButton.WithCallbackData("Преподаватель", "Lecturer"),
                              InlineKeyboardButton.WithCallbackData("Староста", "Headman"),
                          },
                    }
                );


            await client.EditMessageText(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        "давай узнаем кто ты");
            await client.EditMessageReplyMarkup(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        replyMarkup: chooseIndBT);
        }
    }
}
