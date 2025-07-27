using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Interface
{

    internal interface IBotCommand
    {

        Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken);

        Task ErorHendler();

    }
}
