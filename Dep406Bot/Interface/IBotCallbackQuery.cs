using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Interface
{
    internal interface IBotCallbackQuery
    {

        Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken);

        Task ErorHendler();

    }
}
