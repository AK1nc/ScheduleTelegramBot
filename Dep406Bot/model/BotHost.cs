using Dep406Bot.CustomAttribute;
using Dep406Bot.Data;
using Dep406Bot.Data.Interface;
using Dep406Bot.Interface;
using Dep406Bot.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dep406Bot.model
{
    public class BotHost(IHttpAPIClient APIclient, IChatHistory DBChatHistory, string Token) : IHostedService, IDisposable
    {
        //// Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        //private ITelegramBotClient _botClient;

        //// Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        //private ReceiverOptions _receiverOptions;

        // словарь с набором команд
        private Dictionary<string, Type> _keyCommand = new Dictionary<string, Type>();
        private Dictionary<string, Type> _keyCallbackQuery = new Dictionary<string, Type>();    

        private ITelegramBotClient _botClient = new TelegramBotClient(Token); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
        private ReceiverOptions _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
            {
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                UpdateType.CallbackQuery,
                }
};

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
//public BotHost()
//        {
//            _botClient = new TelegramBotClient(Token); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
//            _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
//            {
//                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
//                {
//                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
//            }
//            };
//        }


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
                                            ConstructorInfo cinfo = comand.GetConstructor(new Type[] {typeof(IChatHistory), typeof(IHttpAPIClient) });
                                            if (cinfo == null) {
                                                cinfo = comand.GetConstructor(new Type[] { });
                                                var command_obj = cinfo.Invoke(new object[] { });
                                                comand.GetMethod("Realization").Invoke(command_obj, [botClient, update, null]);
                                            }
                                            else
                                            {
                                                var command_obj = cinfo.Invoke(new object[] {DBChatHistory, APIclient });

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
                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            string[] data = callbackQuery.Data.Split("/");
                            var bt = _keyCallbackQuery[data[0].ToString()];
                            ConstructorInfo cinfo = bt.GetConstructor(new Type[] { typeof(IChatHistory) });


                            if (cinfo == null)
                            {
                                cinfo = bt.GetConstructor(new Type[] { });

                                if(cinfo == null)
                                {
                                    cinfo = bt.GetConstructor(new Type[] { typeof(IChatHistory), typeof(IHttpAPIClient) });
                                    var command_Obj = cinfo.Invoke(new object[] { DBChatHistory, APIclient });
                                    bt.GetMethod("Realization").Invoke(command_Obj, [botClient, update, null]);
                                }

                                var command_obj = cinfo.Invoke(new object[] { });

                                bt.GetMethod("Realization").Invoke(command_obj, [botClient, update, null]);
                            }
                            else
                            {
                                var command_obj = cinfo.Invoke(new object[] { DBChatHistory });

                                bt.GetMethod("Realization").Invoke(command_obj, [botClient, update, null]);
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

        private void ReflexsionConfigurateCommand(Type intrType, IDictionary enm) 
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            DescriptionAttribute descriptionAttribute;

            BotCommandsAtribute customAtribute;

            //Type interfaceType = intrType;

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (intrType.IsAssignableFrom(type) && type.IsClass)
                        {
                            descriptionAttribute = (DescriptionAttribute)type.GetCustomAttribute(typeof(DescriptionAttribute), false);
                            if(descriptionAttribute == null) 
                            {
                                customAtribute = (BotCommandsAtribute)type.GetCustomAttribute(typeof(BotCommandsAtribute), false);
                                foreach(string key in customAtribute.CommandMassive) 
                                {
                                    enm.Add(key, type);
                                }
                            }
                            else
                            {
                                enm.Add(descriptionAttribute.Description, type);
                            }

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // формируем словари команд и кнопок
            ReflexsionConfigurateCommand(typeof(IBotCommand), _keyCommand);
            ReflexsionConfigurateCommand(typeof(IBotCallbackQuery), _keyCallbackQuery);

            using var cts = new CancellationTokenSource();

            // UpdateHander - обработчик приходящих Update`ов
            // ErrorHandler - обработчик ошибок, связанных с Bot API
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMe(); // Создаем переменную, в которую помещаем информацию о нашем боте.

            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
