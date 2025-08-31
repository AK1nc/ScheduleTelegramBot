using Dep406Bot.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dep406Bot.Data.Interface
{
    public interface IChatHistory
    {
        /// <summary>
        /// создание истории чата (в случае новых пользователей)
        /// </summary>
        /// <param name="chatHistory"></param>
        /// <returns></returns>
        public Task<bool> createChatHistory(long chatId);

        #region get ры

        /// <summary>
        /// получение инстории чата
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<EntityChatHistory?> getChatHistory(long chatId);


        public Task<string> getGroup(long chatId);



        #endregion


        #region set ры

        /// <summary>
        /// установка и обновленеи кафедры
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public Task<bool> setDep(long chatId, string value);
        /// <summary>
        /// установка и обновление группы
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public Task<bool> setGroup(long chatId, string value);
        /// <summary>
        /// установление индификатора
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public Task<bool> setIndificator(long chatId, int value);
        /// <summary>
        /// установление года обучения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<bool> setYear(long chatId, int value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<bool> setIsHandman(long chatId, bool value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<bool> setFaculty(long chatId, int value);

        #endregion


        #region checkers

        /// <summary>
        /// true - старый пользователь
        /// false - новый пользователь
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public Task<bool> isHaveChatHistory(long chatId);

        /// <summary>
        /// проверка флага старосты
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public Task<bool> isHandman(long chatId);

        #endregion

    }
}
