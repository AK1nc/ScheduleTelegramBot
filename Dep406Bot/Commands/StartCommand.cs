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

namespace Dep406Bot.Commands
{
    [Description("/start")]
    internal class StartCommand(IChatHistory _db, IHttpAPIClient APIclient) : IBotCommand
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {

            var isCreate = await _db.createChatHistory(update.Message.Chat.Id);
            

            Console.WriteLine(isCreate.ToString());


            var inlineKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Давай узнаем кто ты", "ShooseIndificator"));


            await client.SendMessage(
                update.Message.Chat.Id,
                "Привет, давай я объясню тебе свои возможности:\n" +
                "/opt - команда для открытия моей настройки\n" +
                "\t Команды настроек клавиатуры \n" +
                "\tУчебный процесс\n" +
                "/news\n\n" +
                "/about_creaters - информация о создателях",
                replyMarkup: inlineKeyboard);
            return;

        }
    }
}
