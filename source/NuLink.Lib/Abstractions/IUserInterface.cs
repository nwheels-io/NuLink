using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NuLink.Lib.Common;

namespace NuLink.Lib.Abstractions
{
    public interface IUserInterface
    {
        void Track(VerbosityLevel level, Expression<Func<string>> message, Action action);
        Task TrackAsync(VerbosityLevel level, Expression<Func<string>> message, Func<Task> action);
        T Track<T>(VerbosityLevel level, Expression<Func<string>> message, Func<T> action);
        Task<T> TrackAsync<T>(VerbosityLevel level, Expression<Func<string>> message, Func<Task<T>> action);
        void Report(VerbosityLevel level, Expression<Func<string>> message);
    }

    public enum VerbosityLevel
    {
        Low,
        Medium,
        High,
        Success,
        Warning,
        Error
    }

    public static class UserInterfaceExtensions
    {
        public static void ReportWarning(this IUserInterface ui, Expression<Func<string>> message)
        {
            ui.Report(VerbosityLevel.Warning, message);                
        }

        public static void ReportError(this IUserInterface ui, Expression<Func<string>> message)
        {
            ui.Report(VerbosityLevel.Error, message);
        }

        public static void ReportError(this IUserInterface ui, Exception error)
        {
            ui.Report(VerbosityLevel.Error, () => $"Error: {error.Message}");                
        }

        public static void ReportImportant(this IUserInterface ui, Expression<Func<string>> message)
        {
            ui.Report(VerbosityLevel.High, message);                
        }

        public static void ReportMedium(this IUserInterface ui, Expression<Func<string>> message)
        {
            ui.Report(VerbosityLevel.Medium, message);                
        }
        
        public static void ReportVerbose(this IUserInterface ui, Expression<Func<string>> message)
        {
            ui.Report(VerbosityLevel.Low, message);                
        }

        public static void TrackImportant(this IUserInterface ui, Expression<Func<string>> message, Action action)
        {
            ui.Track(VerbosityLevel.Low, message, action);
        }

        public static void TrackMedium(this IUserInterface ui, Expression<Func<string>> message, Action action)
        {
            ui.Track(VerbosityLevel.Medium, message, action);
        }
        
        public static void TrackVerbose(this IUserInterface ui, Expression<Func<string>> message, Action action)
        {
            ui.Track(VerbosityLevel.High, message, action);
        }

        public static Task TrackAsyncImportant(this IUserInterface ui, Expression<Func<string>> message, Func<Task> action)
        {
            return ui.TrackAsync(VerbosityLevel.Low, message, action);
        }

        public static Task TrackAsyncMedium(this IUserInterface ui, Expression<Func<string>> message, Func<Task> action)
        {
            return ui.TrackAsync(VerbosityLevel.Medium, message, action);
        }
        
        public static Task TrackAsyncVerbose(this IUserInterface ui, Expression<Func<string>> message, Func<Task> action)
        {
            return ui.TrackAsync(VerbosityLevel.High, message, action);
        }

        public static T TrackImportant<T>(this IUserInterface ui, Expression<Func<string>> message, Func<T> action)
        {
            return ui.Track<T>(VerbosityLevel.Low, message, action);
        }

        public static T TrackMedium<T>(this IUserInterface ui, Expression<Func<string>> message, Func<T> action)
        {
            return ui.Track<T>(VerbosityLevel.Medium, message, action);
        }
        
        public static T TrackVerbose<T>(this IUserInterface ui, Expression<Func<string>> message, Func<T> action)
        {
            return ui.Track<T>(VerbosityLevel.High, message, action);
        }

        public static Task<T> TrackAsyncImportant<T>(this IUserInterface ui, Expression<Func<string>> message, Func<Task<T>> action)
        {
            return ui.TrackAsync<T>(VerbosityLevel.Low, message, action);
        }

        public static Task<T> TrackAsyncMedium<T>(this IUserInterface ui, Expression<Func<string>> message, Func<Task<T>> action)
        {
            return ui.TrackAsync<T>(VerbosityLevel.Medium, message, action);
        }
        
        public static Task<T> TrackAsyncVerbose<T>(this IUserInterface ui, Expression<Func<string>> message, Func<Task<T>> action)
        {
            return ui.TrackAsync<T>(VerbosityLevel.High, message, action);
        }
    }

}
