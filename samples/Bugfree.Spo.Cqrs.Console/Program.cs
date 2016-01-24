using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Console
{
    class Program
    {
        static IEnumerable<ICommand> GetCommands()
        {
            var iCommand = typeof(ICommand);
            return System.Reflection.Assembly.GetExecutingAssembly().GetTypes().ToList()
                .Where(t => iCommand.IsAssignableFrom(t) && t != iCommand)
                .Select(t => Activator.CreateInstance(t) as ICommand);
        }

        static void DisplayHelp()
        {
            System.Console.WriteLine("Console [Command] [Arg1] [Arg2] [ArgN]\n\n");
            GetCommands().ToList().ForEach(command =>
                System.Console.WriteLine(command.Usage + "\n" + command.Description + "\n\n"));
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                return (int)ExitCode.Failure;
            }

            var commandName = args[0];
            var command = GetCommands().SingleOrDefault(t => t.GetType().Name == commandName);
            if (command == null)
            {
                throw new ArgumentException(string.Format("Command '{0}' not found", commandName));
            }

            var executeArguments = new List<string>(args);
            executeArguments.RemoveAt(0);

            var exitCode = command.Execute(executeArguments.ToArray());
            return (int)exitCode;
        }
    }
}
