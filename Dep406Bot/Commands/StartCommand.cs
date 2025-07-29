using Dep406Bot.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Commands
{
    [Description("/start")]
    internal class StartCommand : IBotCommand
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                            update.Message.Chat.Id,
                            "Привет, давай узнаем что тебе нужно\n" +
                            ""
                            );
        }
    }
}
