using LibGit2Sharp;

namespace NuLink.Lib.Workspaces
{
    public class PackageMetadata
    {
        public PackageMetadata(PackageReference reference, string sourceRepoType, string sourceRepoUrl)
        {
            Reference = reference;
            SourceRepoType = sourceRepoType;
            SourceRepoUrl = sourceRepoUrl;
        }

        public PackageReference Reference { get; }
        public string SourceRepoType { get; }
        public string SourceRepoUrl { get; }
    }
}
