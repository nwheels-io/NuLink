using System;
using System.Linq.Expressions;

namespace NuLink.Cli
{
    public interface IUserInterface
    {
        void Report(
            VerbosityLevel level, 
            Expression<Func<string>> message, 
            params ConsoleColor[] paramColors);
    }
    
    public enum VerbosityLevel
    {
        Low,
        Medium,
        High,
        Success,
        Warning,
        Error,
        Data
    }

    public static class UserInterfaceExtensions
    {
        public static void ReportLow(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Low, message, paramColors);
        }

        public static void ReportMedium(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Medium, message, paramColors);
        }

        public static void ReportHigh(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.High, message, paramColors);
        }

        public static void ReportSuccess(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Success, message, paramColors);
        }

        public static void ReportWarning(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Warning, message, paramColors);
        }

        public static void ReportError(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Error, message, paramColors);
        }

        public static void ReportError(this IUserInterface ui, Exception error)
        {
            ui.Report(VerbosityLevel.Error, () => $"Error: {error.Message}");
        }

        public static void ReportData(this IUserInterface ui, Expression<Func<string>> message, params ConsoleColor[] paramColors)
        {
            ui.Report(VerbosityLevel.Data, message, paramColors);
        }
    }
}
