using Dep406Bot.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.model
{
    public class BotHost
    {
        // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        private ITelegramBotClient _botClient;

        // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        private ReceiverOptions _receiverOptions;

        // словарь с набором команд
        private Dictionary<string, Type> _keyCommand = new Dictionary<string, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Token"></param>
        public BotHost(string Token)
        {
            _botClient = new TelegramBotClient(Token); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
            _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
            {
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
            }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task BotStart()
        {

            ReflexsionConfigurateCommand();

            using var cts = new CancellationTokenSource();

            // UpdateHander - обработчик приходящих Update`ов
            // ErrorHandler - обработчик ошибок, связанных с Bot API
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMe(); // Создаем переменную, в которую помещаем информацию о нашем боте.

            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
        }


        /// <summary>
        /// Слушатель обновлений
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            try
            {
                // Обработка update по типу
                switch (update.Type)
                {
                    // если это сообщение
                    case UpdateType.Message:
                        {

                            var message = update.Message;


                            var user = message.From;


                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");


                            var chat = message.Chat;

                            // Обработка message по типу
                            switch (message.Type)
                            {
                                // Если message текстовый тип
                                case MessageType.Text:
                                    {
                                        var mes = message.Text.Split(" ");
                                        try { 
                                            var comand = _keyCommand[mes[0]];

                                            if(comand != null) { 
                                                var command_obj = Activator.CreateInstance(comand);

                                                comand.GetMethod("Realization").Invoke(command_obj, [botClient, update, null]);
                                            }
                                        }
                                        catch {}
                                        #region Обработка текстовых команд
                                        if (message.Text == "/opt")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                "Выбери клавиатуру:\n" +
                                                "/inline\n" +
                                                "/reply\n");
                                            return;
                                        }
                                        if (message.Text == "/news")
                                        {
                                            await botClient.SendMessage(
                                                       chat.Id,
                                                       "Здесь появится новость"
                                                       );
                                            return;
                                        }
                                        if (message.Text == "/inline")
                                        {
                                            // Тут создаем нашу клавиатуру
                                            var inlineKeyboard = new InlineKeyboardMarkup(
                                                new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                                                {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("Это кнопка с сайтом", "https://habr.com/"),
                                            InlineKeyboardButton.WithCallbackData("А это просто кнопка", "button1"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Тут еще одна", "button2"),
                                            InlineKeyboardButton.WithCallbackData("И здесь", "button3"),
                                        },
                                                });

                                            await botClient.SendMessage(
                                                chat.Id,
                                                "Это inline клавиатура!",
                                                replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup

                                            return;
                                        }

                                        if (message.Text == "/reply")
                                        {
                                            // Тут все аналогично Inline клавиатуре, только меняются классы
                                            // НО! Тут потребуется дополнительно указать один параметр, чтобы
                                            // клавиатура выглядела нормально, а не как абы что

                                            var replyKeyboard = new ReplyKeyboardMarkup(
                                                new List<KeyboardButton[]>()
                                                {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Привет!"),
                                            new KeyboardButton("Пока!"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Позвони мне!")
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Напиши моему соседу!")
                                        }
                                                })
                                            {
                                                // автоматическое изменение размера клавиатуры, если не стоит true,
                                                // тогда клавиатура растягивается чуть ли не до луны,
                                                // проверить можете сами
                                                ResizeKeyboard = true,
                                            };

                                            await botClient.SendMessage(
                                                chat.Id,
                                                "Это reply клавиатура!",
                                                replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup

                                            return;
                                        }

                                        return;
                                        #endregion
                                    }
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        /// <summary>
        /// ErrorHandler слушатель для возникновения ошибки
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="error"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // код ошибки
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        private void ReflexsionConfigurateCommand() 
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            DescriptionAttribute descriptionAttribute;

            Type interfaceType = typeof(IBotCommand);

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (interfaceType.IsAssignableFrom(type) && type.IsClass)
                        {
                            descriptionAttribute = (DescriptionAttribute)type.GetCustomAttribute(typeof(DescriptionAttribute), false);
                            _keyCommand.Add(descriptionAttribute.Description, type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"Ошибка загрузки типов из сборки {assembly.FullName}: {ex.Message}");
                    // Можно обработать исключение или пропустить сборку
                }
            }

        }



    }
}
