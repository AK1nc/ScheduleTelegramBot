using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dep406Bot.model;
using Microsoft.EntityFrameworkCore;

namespace Dep406Bot.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public DbSet<EntityChatHistory> ChatHistories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {

        }

    }
}
