using System.IO;
using NuLink.Lib.Abstractions;
using NuLink.Lib.SourceControls.Git;

namespace NuLink.Lib.SourceControls
{
    public class AutoDetectingSourceControl : ISourceControl
    {
        public ISourceRepository GetRepoOfDirectory(DirectoryInfo directory)
        {
            return new GitSourceRepository();
        }
    }
}