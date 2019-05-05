using System;

namespace NuLink.Cli
{
    public class NuLinkCommandOptions
    {
        public NuLinkCommandOptions(string projectPath, string packageId, bool dryRun)
        {
            ProjectPath = projectPath;
            PackageId = packageId;
            DryRun = dryRun;
            ProjectIsSolution = ProjectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase);
        }

        public string ProjectPath { get; }
        public bool ProjectIsSolution { get; }
        public string PackageId { get; }
        public bool DryRun { get; }
    }
}