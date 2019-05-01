using System.IO;

namespace NuLink.Lib.Abstractions
{
    public interface ISourceControl
    {
        ISourceRepository GetRepoOfDirectory(DirectoryInfo directory);
    }

    public interface ISourceRepository
    {
        
    }

    public class CodeAuthor
    {
    }
}