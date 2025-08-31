using Dep406Bot.CustomAttribute;
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
    [BotCommandsAtribute(["1year", "2year", "3year", "4year", "5year"])]
    internal class ChooseYearBT(IChatHistory _db) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {

            try
            {
                await _db.setYear(update.CallbackQuery.Message.Chat.Id, Convert.ToInt16(update.CallbackQuery.Data[0].ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var chooseFacultyBT = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("1 институт", "1faculty"),
                        InlineKeyboardButton.WithCallbackData("2 институт", "2faculty"),
                        InlineKeyboardButton.WithCallbackData("3 институт", "3faculty"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("4 институт", "4faculty"),
                        InlineKeyboardButton.WithCallbackData("5 институт", "5faculty"),
                        InlineKeyboardButton.WithCallbackData("6 институт", "6faculty"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("7 институт", "7faculty"),
                        InlineKeyboardButton.WithCallbackData("8 институт", "8faculty"),
                        InlineKeyboardButton.WithCallbackData("9 институт", "9faculty"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("10 институт", "10faculty"),
                        InlineKeyboardButton.WithCallbackData("11 институт", "11faculty"),
                        InlineKeyboardButton.WithCallbackData("12 институт", "12faculty"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("13 институт", "13faculty"),
                        InlineKeyboardButton.WithCallbackData("14 институт", "14faculty"),
                    },
                }
            );

            await client.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                "Факультет обучения:");
            await client.EditMessageReplyMarkup(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                replyMarkup: chooseFacultyBT);
            return;
        }
    }
}
