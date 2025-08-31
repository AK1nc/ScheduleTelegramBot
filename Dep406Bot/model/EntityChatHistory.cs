using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dep406Bot.model
{
    public class EntityChatHistory
    {
        public int Id { get; set; }

        public long ChatId { get; set; }

        public string? ClientId { get; set; }

        public string? Nickname { get; set; }


        // TODO придумать одельные типы для разных людей 
        /// <summary>
        /// 0 - если у пользователя нет регистрации<br/>
        /// 1 - студент бакалавриата => есть название группы<br/>
        /// 2 - студент специалитета
        /// 3 - студент магистратуры
        /// 4 - студет аспирантуры
        /// 5 - преподаватель
        /// Можно будет выбрать если преподаватель еще не получил акредитацию<br/>
        /// 6 - зарегистрированный преподаватель (с подтверждением)
        /// зарегистрированый преподаватель сможет изменять кабинеты для расписания, назначаить новые занятия и отменять их<br/>
        /// сможет добавлять заметки и дз к расписанию<br/>
        /// </summary>
        public int? Indificator { get; set; }

        /// <summary>
        /// true - староста
        /// false - не староста
        /// null - не студент
        /// </summary>
        public bool? isHandman { get; set; } = false;

        /// <summary>
        /// группа в формате М0О-000С-00
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// кафедра в формате 000
        /// </summary>
        public string? Dep {  get; set; }

        public int? Faculty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? Year { get; set; }

        ///дтвержденный или не подтвержденный староста или преподаватель
        public bool isConfirmed { get; set; } = false;

    }
}
