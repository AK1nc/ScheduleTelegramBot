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

namespace Dep406Bot.Buttons.Indificators
{
    [Description("Lecturer")]
    internal class LecturerBT(IChatHistory _db) : IBotCallbackQuery
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {

            var chooseYeBT = new InlineKeyboardMarkup
               (
                   new List<InlineKeyboardButton[]>()
                   {
                          new InlineKeyboardButton[]
                          {
                              InlineKeyboardButton.WithCallbackData("1 курс", "BachelorStudent"),
                              InlineKeyboardButton.WithCallbackData("2 курс", "SpecialistStudent"),
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
                        replyMarkup: chooseYeBT);
            return;
        }
    }
}
