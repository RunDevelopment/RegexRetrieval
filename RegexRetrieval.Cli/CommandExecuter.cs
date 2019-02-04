using System;
using System.Collections.Generic;
using System.Linq;

namespace RegexRetrieval.Cli
{
    public class CommandExecuter
    {
        private readonly List<Command> commands = new List<Command>();

        public void AddCommand(Command command)
            => commands.Add(command);
        public void AddCommand(string name, Action<string[]> action)
            => AddCommand(new Command(name, null, action));

        public bool TryExecute(string commandString)
        {
            var parts = commandString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return false;

            var name = parts[0];
            foreach (var command in commands)
            {
                if (name == command.Name)
                {
                    command.Action(parts.Skip(1).ToArray());
                    return true;
                }
            }

            return false;
        }


        public class Command
        {
            public string Name { get; }
            public string Desc { get; }
            public Action<string[]> Action { get; }

            public Command(string name, string desc, Action<string[]> action)
            {
                Name = name;
                Desc = desc;
                Action = action;
            }
        }
    }
}
