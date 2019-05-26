using System;
using System.Linq.Expressions;
using System.Net.WebSockets;

namespace NuLink.Cli
{
    public class BareUI : IUserInterface
    {
        public void Report(
            VerbosityLevel level, 
            Expression<Func<string>> message, 
            params ConsoleColor[] paramColors)
        {
            switch (level)
            {
                case VerbosityLevel.Error:
                    Console.Error.WriteLine(message.Compile().Invoke());
                    break;
                case VerbosityLevel.Data:
                    Console.WriteLine(message.Compile().Invoke());
                    break;
            }
        }
    }
}
