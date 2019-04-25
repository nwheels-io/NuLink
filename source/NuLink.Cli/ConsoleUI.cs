using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NuLink.Lib.Abstractions;
using NuLink.Lib.Common;

namespace NuLink.Cli
{
    public class ConsoleUI : IUserInterface
    {
        private static readonly IReadOnlyDictionary<VerbosityLevel, Palette> PaletteByLevel = 
            new Dictionary<VerbosityLevel, Palette> {
                [VerbosityLevel.Error] = new Palette(
                    message: ConsoleColor.Red, 
                    connectingLiteral: ConsoleColor.DarkRed,
                    argument: ConsoleColor.Yellow,
                    alternateArgument: ConsoleColor.Magenta),
                [VerbosityLevel.Warning] = new Palette(
                    message: ConsoleColor.Yellow, 
                    connectingLiteral: ConsoleColor.DarkYellow,
                    argument: ConsoleColor.White,
                    alternateArgument: ConsoleColor.Cyan),
                [VerbosityLevel.High] = new Palette(
                    message: ConsoleColor.White, 
                    connectingLiteral: ConsoleColor.Gray,
                    argument: ConsoleColor.Green,
                    alternateArgument: ConsoleColor.Cyan),
                [VerbosityLevel.Medium] = new Palette(
                    message: ConsoleColor.Gray, 
                    connectingLiteral: ConsoleColor.DarkGray,
                    argument: ConsoleColor.Green,
                    alternateArgument: ConsoleColor.Cyan),
                [VerbosityLevel.Low] = new Palette(
                    message: ConsoleColor.DarkGray, 
                    connectingLiteral: ConsoleColor.DarkGray,
                    argument: ConsoleColor.White,
                    alternateArgument: ConsoleColor.Gray),
            };
        
        public void Track(VerbosityLevel level, Expression<Func<string>> message, Action action)
        {
            throw new NotImplementedException();
        }

        public Task TrackAsync(VerbosityLevel level, Expression<Func<string>> message, Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public T Track<T>(VerbosityLevel level, Expression<Func<string>> message, Func<T> action)
        {
            throw new NotImplementedException();
        }

        public Task<T> TrackAsync<T>(VerbosityLevel level, Expression<Func<string>> message, Func<Task<T>> action)
        {
            throw new NotImplementedException();
        }

        public void Report(VerbosityLevel level, Expression<Func<string>> message)
        {
            WriteFormatColors(message, PaletteByLevel[level]);
            Console.WriteLine();
        }

        public static void FatalError(Expression<Func<string>> message)
        {
            Console.WriteLine();
            WriteFormatColors(message, PaletteByLevel[VerbosityLevel.Error]);
            Console.WriteLine();
        }
        
        private static void WriteFormatColors(Expression<Func<string>> message, Palette palette)
        {
            var interpolated = new InterpolatedString(message);
            var argumentIndex = 0;
            var saveColor = Console.ForegroundColor;
            
            for (int partIndex = 0; partIndex < interpolated.FormatParts.Count; partIndex++)
            {
                var isArgument = (partIndex % 2) == 1;
                string partText;
                ConsoleColor partColor;
                
                if (isArgument)
                {
                    var formatSpec = interpolated.FormatParts[partIndex];
                    partText = string.Format(
                        "{0}" + (formatSpec.Length > 0 ? ":" + formatSpec : ""),
                        interpolated.FormatArgs[argumentIndex]);
                    partColor = ((argumentIndex % 2) == 0 ? palette.Argument : palette.AlternateArgument);
                    argumentIndex++;
                }
                else
                {
                    partText = interpolated.FormatParts[partIndex];
                    partColor = (partIndex == 0 ? palette.Message : palette.ConnectingLiteral);
                }

                Console.ForegroundColor = partColor;
                Console.Write(partText);
            }

            Console.ForegroundColor = saveColor;
        }

        public class Palette
        {
            public Palette(ConsoleColor message, ConsoleColor connectingLiteral, ConsoleColor argument, ConsoleColor alternateArgument)
            {
                Message = message;
                ConnectingLiteral = connectingLiteral;
                Argument = argument;
                AlternateArgument = alternateArgument;
            }

            public ConsoleColor Message { get; }
            public ConsoleColor ConnectingLiteral { get; }
            public ConsoleColor Argument { get; }
            public ConsoleColor AlternateArgument { get; }
        }
    }
}