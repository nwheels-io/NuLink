using System;
using System.IO;

namespace NuLink.Tests.Acceptance
{
    public abstract class AcceptanceTestTarget
    {
        public abstract string ConsumerSolutionFolder { get; }
        public abstract string ConsumerSolutionFile { get; }
        public abstract string PackageProjectFolder(string packageId);
        public abstract string PackageProjectFile(string packageId);
        public abstract string PackagesRootFolder { get; }
        public abstract string PackageNugetFolder(string packageId);
        public abstract string PackageNugetLibFolder(string packageId, string version);
        
        public static readonly AcceptanceTestTarget NetCore = new NetCoreTestTarget(); 
        //public static readonly AcceptanceTestTarget NetFx = new NetFxTestTarget();

        public static AcceptanceTestTarget Get(TargetProjectKind projectKind)
        {
            switch (projectKind)
            {
                case TargetProjectKind.NetCore:
                    return NetCore; 
            }
            
            throw new NotSupportedException(projectKind.ToString());
        }
    }

    public class NetCoreTestTarget : AcceptanceTestTarget
    {
        public override string ConsumerSolutionFolder =>
            Path.Combine(TestEnvironment.DemoFolder, "NuLink.TestCase.Consumer");
        public override string ConsumerSolutionFile => 
            Path.Combine(ConsumerSolutionFolder, "NuLink.TestCase.Consumer.sln");
        public override string PackageProjectFolder(string packageId) => Path.Combine(
            TestEnvironment.DemoFolder, 
            packageId,
            packageId);
        public override string PackageProjectFile(string packageId) =>
            Path.Combine(PackageProjectFolder(packageId), $"{packageId}.csproj");
        public override string PackagesRootFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
        public override string PackageNugetFolder(string packageId) => Path.Combine(
            PackagesRootFolder,
            packageId.ToLower());
        public override string PackageNugetLibFolder(string packageId, string version) => Path.Combine(
            PackageNugetFolder(packageId),
            version,
            "lib");
    }
}