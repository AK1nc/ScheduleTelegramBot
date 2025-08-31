using Dep406Bot.Data.Interface;
using Dep406Bot.model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dep406Bot.Data
{
    internal class ChatHistoryDB : IChatHistory
    {

        public readonly ApplicationDbContext _context;

        public ChatHistoryDB(ApplicationDbContext dbContex)
        {
            _context = dbContex;
        }

        public async Task<bool> createChatHistory(long chatId)
        {
            EntityChatHistory newChatHistory = new();

            newChatHistory.ChatId = chatId;

            _context.ChatHistories.Add(newChatHistory);

            _context.SaveChanges();

            return true;
        }

        public async Task<EntityChatHistory?> getChatHistory(long chatId)
        {
            var ChatHistory = await _context.ChatHistories.FirstAsync(c => c.ChatId == chatId);

            //var temp = new EntityChatHistory();
            //temp.ChatId = ChatHistory.ChatId;
            //temp.ClientId = ChatHistory.ClientId;
            //temp.Nickname = temp.Nickname;
            //temp.isConfirmed = temp.isConfirmed;
            //temp.Dep = temp.Dep;
            //temp.Faculty = temp.Faculty;
            //temp.Group = temp.Group;
            //temp.


            return ChatHistory;
        }

        public async Task<bool> isHandman(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> isHaveChatHistory(long chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> setDep(long chatId, string value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.Dep = value;

            _context.ChatHistories.Entry(tempEn).Property(x => x.Dep).IsModified = true;

            _context.SaveChanges();

            return true;
        }

        public async Task<bool> setGroup(long chatId, string value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.Group = value;

            _context.ChatHistories.Entry(tempEn).Property(x => x.Group).IsModified = true;

            _context.SaveChanges();

            return true;
        }

        public  async Task<bool> setIndificator(long chatId, int value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.Indificator = value;

            _context.ChatHistories.Entry(tempEn).Property(x => x.Indificator).IsModified = true;

            _context.SaveChanges();

            return true;
        }
                
        public  async Task<bool> setIsHandman(long chatId, bool value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.isHandman = value;

            _context.ChatHistories.Entry(tempEn)
                                  .Property(x => x.isHandman)
                                  .IsModified = true;

            _context.SaveChanges();

            return true;
        }
                
        public  async Task<bool> setYear(long chatId, int value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.Year = value;

            _context.ChatHistories.Entry(tempEn)
                                  .Property(x => x.Year)
                                  .IsModified = true;

            _context.SaveChanges();

            return true;
        }

        //TODO  async
        public  async Task<bool> setVariabel(long chatId, string parament, Type T, string value)
        {

            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            _context.ChatHistories.Entry(tempEn)
                                  .Property(x => x.Year)
                                  .IsModified = true;

            _context.SaveChanges();

            return true;
        }

        public  async Task<bool> setFaculty(long chatId, int value)
        {
            var tempEn = _context.ChatHistories.FirstOrDefault(_context => _context.ChatId == chatId);

            tempEn.Faculty = value;

            _context.ChatHistories.Entry(tempEn)
                                  .Property(x => x.Faculty)
                                  .IsModified = true;

            _context.SaveChanges();

            return true;
        }

        public async Task<string> getGroup(long chatId)
        {
            var tempEn = _context.ChatHistories
                                 .FirstOrDefault(_context => _context.ChatId == chatId)
                                 .Group
                                 .ToString();

            return tempEn;
        }
    }
}
