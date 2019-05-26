using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NuLink.Cli
{
    public class FullUI : IUserInterface
    {
        private static readonly IReadOnlyDictionary<VerbosityLevel, Palette> PaletteByLevel = 
            new Dictionary<VerbosityLevel, Palette> {
                [VerbosityLevel.Data] = new Palette(
                    message: ConsoleColor.White, 
                    connector: ConsoleColor.Gray,
                    data: ConsoleColor.Magenta,
                    alternateData: ConsoleColor.Cyan),
                [VerbosityLevel.Success] = new Palette(
                    message: ConsoleColor.Green, 
                    connector: ConsoleColor.Green,
                    data: ConsoleColor.Cyan,
                    alternateData: ConsoleColor.Cyan),
                [VerbosityLevel.Error] = new Palette(
                    message: ConsoleColor.Red, 
                    connector: ConsoleColor.DarkRed,
                    data: ConsoleColor.Yellow,
                    alternateData: ConsoleColor.Magenta),
                [VerbosityLevel.Warning] = new Palette(
                    message: ConsoleColor.Yellow, 
                    connector: ConsoleColor.DarkYellow,
                    data: ConsoleColor.White,
                    alternateData: ConsoleColor.Cyan),
                [VerbosityLevel.High] = new Palette(
                    message: ConsoleColor.White, 
                    connector: ConsoleColor.Gray,
                    data: ConsoleColor.Green,
                    alternateData: ConsoleColor.Cyan),
                [VerbosityLevel.Medium] = new Palette(
                    message: ConsoleColor.Gray, 
                    connector: ConsoleColor.DarkGray,
                    data: ConsoleColor.Green,
                    alternateData: ConsoleColor.Cyan),
                [VerbosityLevel.Low] = new Palette(
                    message: ConsoleColor.DarkGray, 
                    connector: ConsoleColor.DarkGray,
                    data: ConsoleColor.White,
                    alternateData: ConsoleColor.Gray),
            };
        
        public void Report(VerbosityLevel level, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            WriteFormatColors(message, PaletteByLevel[level], paramColors);
            Console.WriteLine();
        }
        
        public static void FatalError(Expression<Func<string>> message)
        {
            Console.WriteLine();
            WriteFormatColors(message, PaletteByLevel[VerbosityLevel.Error]);
            Console.WriteLine();
            Console.WriteLine();
        }
        
        private static void WriteFormatColors(
            Expression<Func<string>> message, 
            Palette palette, 
            params ConsoleColor[] paramColors)
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

                    if (argumentIndex < paramColors.Length)
                    {
                        partColor = paramColors[argumentIndex];
                    }
                    else
                    {
                        partColor = ((argumentIndex % 2) == 0 ? palette.Data : palette.AlternateData);
                    }
                        
                    argumentIndex++;
                }
                else
                {
                    partText = interpolated.FormatParts[partIndex];
                    partColor = (partIndex == 0 ? palette.Message : palette.Connector);
                }

                Console.ForegroundColor = partColor;
                Console.Write(partText);
            }

            Console.ForegroundColor = saveColor;
        }

        public class Palette
        {
            public Palette(ConsoleColor message, ConsoleColor connector, ConsoleColor data, ConsoleColor alternateData)
            {
                Message = message;
                Connector = connector;
                Data = data;
                AlternateData = alternateData;
            }

            public ConsoleColor Message { get; }
            public ConsoleColor Connector { get; }
            public ConsoleColor Data { get; }
            public ConsoleColor AlternateData { get; }
        }
    }
}
