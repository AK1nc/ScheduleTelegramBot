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

namespace Dep406Bot.Buttons.Schedule
{
    [Description("NextWeek")]
    internal class NextWeek(IChatHistory _db, IHttpAPIClient _api) : IBotCallbackQuery
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
