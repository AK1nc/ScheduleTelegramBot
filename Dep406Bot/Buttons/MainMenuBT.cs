using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons
{
    [Description("MainMenu")]
    internal class MainMenuBT(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {

            var studentMenuBT = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Расписание", "Schedule"),
                        InlineKeyboardButton.WithCallbackData("Задания", "Task"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Кабинеты", "MasterStudent"),
                        InlineKeyboardButton.WithCallbackData("Профиль", "Profile"),
                    },
                }
            );

            await client.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                "");
            await client.EditMessageReplyMarkup(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                replyMarkup: studentMenuBT);
            return;

        }
    }
}
