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
    /// <summary>
    /// староста 
    /// </summary>
    [Description("Headman")]
    internal class HeadmanBT(IChatHistory _db) : IBotCallbackQuery
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            // ставим флаг старосты
            await _db.setIsHandman(update.CallbackQuery.Message.Chat.Id, true);

            var chooseYeBT = new InlineKeyboardMarkup
                (
                    new List<InlineKeyboardButton[]>()
                    {
                          new InlineKeyboardButton[]
                          {
                              InlineKeyboardButton.WithCallbackData("студент бакалавриата", "BachelorStudent"),
                              InlineKeyboardButton.WithCallbackData("студент специалитета", "SpecialistStudent"),
                          },
                          new InlineKeyboardButton[]
                          {
                              InlineKeyboardButton.WithCallbackData("студент маистратуры", "MasterStudent"),
                              InlineKeyboardButton.WithCallbackData("студент аспирантуры", "PostgraduateStudent"),
                          },
                    }
                );

            await client.EditMessageText(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        "Давай узнаем твое направление:");
            await client.EditMessageReplyMarkup(
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        replyMarkup: chooseYeBT);
            return;
        }
    }
}
