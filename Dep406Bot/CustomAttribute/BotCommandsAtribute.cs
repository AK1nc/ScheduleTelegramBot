using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dep406Bot.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BotCommandsAtribute : Attribute
    {
        public string[] CommandMassive { get; }


        public BotCommandsAtribute(string[] _commandMassive) 
        {
            CommandMassive = _commandMassive;
        }

    }
}
