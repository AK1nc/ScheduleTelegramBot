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
    /// студент аспирантуры
    /// </summary>
    [Description("PostgraduateStudent")]
    internal class PostgraduateStudentBT(IChatHistory _db) : IBotCallbackQuery
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await _db.setIndificator(update.CallbackQuery.Message.Chat.Id, 4);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var chooseYeBT = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("1 курс", "1year"),
                        InlineKeyboardButton.WithCallbackData("2 курс", "2year"),
                        InlineKeyboardButton.WithCallbackData("3 курс", "3year"),
                    },
                }
            );

            await client.EditMessageText
                (
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Год твоего обучения:"
                );

            await client.EditMessageReplyMarkup
                (
                        update.CallbackQuery.Message.Chat.Id,
                        update.CallbackQuery.Message.MessageId,
                        replyMarkup: chooseYeBT
                );
            return;
        }
    }
}
