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

namespace Dep406Bot.Buttons.Chooses
{
    [Description("ChooseDep")]
    internal class ChooseDepBT(IChatHistory _db) : IBotCallbackQuery
    {
        public Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
