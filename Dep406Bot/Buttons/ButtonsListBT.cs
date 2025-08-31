using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.Buttons
{
    [Description("ButtonsList")]
    public class ButtonsListBT
    {

        public InlineKeyboardMarkup MainMenuBT = new InlineKeyboardMarkup
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

        public InlineKeyboardMarkup ScheduleMenuBT = new InlineKeyboardMarkup
            (
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Сегодня", "Today"),
                        InlineKeyboardButton.WithCallbackData("Завтра", "Tomorrow"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Неделя", "Week"),
                        InlineKeyboardButton.WithCallbackData("Следующая Неделя", "NextWeek"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("На главную", "MainMenu"),
                    }
                }
            );



    }
}
