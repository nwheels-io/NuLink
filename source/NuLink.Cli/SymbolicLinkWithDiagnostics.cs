using System;
using System.Runtime.Serialization;
using Murphy.SymbolicLink;

namespace NuLink.Cli
{
    public static class SymbolicLinkWithDiagnostics
    {
        public static void Create(string fromPath, string toPath)
        {
            try
            {
                SymbolicLink.create(toPath, fromPath);
            }
            catch (Exception e)
            {
                throw new SymbolicLinkException(e, "create", fromPath: fromPath, toPath: toPath);
            }
        }
        
        public static string Resolve(string toPath)
        {
            try
            {
                return SymbolicLink.resolve(toPath);
            }
            catch (Exception e)
            {
                throw new SymbolicLinkException(e, "resolve", toPath: toPath);
            }
        }
    }

    [Serializable]
    public class SymbolicLinkException : Exception
    {
        public SymbolicLinkException()
        {
        }

        public SymbolicLinkException(
            Exception inner,
            string action,
            string fromPath = null, 
            string toPath = null)
            : base(FormatMessage(action, fromPath, toPath), inner)
        {
        }

        public SymbolicLinkException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SymbolicLinkException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
        
        public static string FormatMessage(string action, string fromPath, string toPath)
        {
            var newl = Environment.NewLine;
            return 
                $"Attempted symbolic link action: {action}{newl}" +
                $"- from path: [{fromPath ?? "N/A"}]{newl}" +
                $"-   to path: [{toPath ?? "N/A"}]";
        }
    }
}