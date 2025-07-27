using Dep406Bot.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dep406Bot.Commands
{
    [Description("/help")]
    internal class HelpCommand : IBotCommand
    {
        public async Task ErorHendler()
        {
            throw new NotImplementedException();
        }

        public async Task Realization(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                            update.Message.Chat.Id,
                            "Привет, давай я объясню тебе свои возможности:\n" +
                            "/opt - команда для открытия моей настройки\n" +
                            "\t Команды настроек клавиатуры \n" +
                            "/inline - клавиатура команд под моим сообщением (рекомендуется)\n" +
                            "/reply -  клавиатура команд под стракой ввода\n\n" +
                            "\tУчебный процесс\n" +
                            "/educational - меню команд помогающих в учебном процессе\n" +
                            "/free_rooms - свободные аудитории на ближайшую пару\n" +
                            "/professor_placement Фамилия И О - расписание преподователя\n\n" +
                            "\t Команда для просмотра новостей \n" +
                            "/news\n\n" +
                            "/about_creaters - информация о создателях"
                            );
        }
    }
}
