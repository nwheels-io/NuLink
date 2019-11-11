using System;

namespace NuLink.Cli
{
    public class NuLinkCommandOptions
    {
        public enum LinkMode
        {
            Single,
            PackageToAll,
            All
        }

        public NuLinkCommandOptions(
            string consumerProjectPath,
            string rootDirectory = null,
            string packageId = null,
            bool dryRun = false,
            bool bareUI = false,
            string localProjectPath = null)
        {
            ConsumerProjectPath = consumerProjectPath;
            RootDirectory = rootDirectory;
            PackageId = packageId;
            DryRun = dryRun;
            BareUI = bareUI;
            LocalProjectPath = localProjectPath;
            ProjectIsSolution = ConsumerProjectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase);
            Mode = rootDirectory != null ? packageId == null ? LinkMode.All : LinkMode.PackageToAll : LinkMode.Single;
        }

        public string ConsumerProjectPath { get; }
        public bool ProjectIsSolution { get; }
        public string RootDirectory { get; }
        public string PackageId { get; }
        public string LocalProjectPath { get; }
        public bool DryRun { get; }
        public bool BareUI { get; }
        public LinkMode Mode { get; }
    }
}