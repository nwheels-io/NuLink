using System;
using System.IO;
using System.Linq;
using Murphy.SymbolicLink;

namespace NuLink.Cli
{
    public class LinkCommand : INuLinkCommand
    {
        public int Execute(NuLinkCommandOptions options)
        {
            Console.WriteLine(
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader();
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);

            var requestedPackage = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);

            if (requestedPackage == null)
            {
                throw new Exception($"Error: Package not referenced: {options.PackageId}");
            }

            var status = requestedPackage.CheckStatus();

            if (!status.LibFolderExists)
            {
                throw new Exception($"Error: Cannot link package {options.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
            }

            if (status.IsLibFolderLinked)
            {
                throw new Exception($"Error: Package {requestedPackage.PackageId} is already linked to {status.LibFolderLinkTargetPath}");
            }

            if (!status.LibBackupFolderExists)
            {
                Directory.Move(requestedPackage.LibFolderPath, requestedPackage.LibBackupFolderPath);
            }
            else
            {
                Console.WriteLine($"Warning: backup folder was not expected to exist: {requestedPackage.LibBackupFolderPath}");
            }

            var linkTargetPath = Path.Combine(Path.GetDirectoryName(options.LocalProjectPath), "bin", "Debug");
            CreateSymbolicLink(linkPath: requestedPackage.LibFolderPath, targetPath: linkTargetPath);
            
            Console.WriteLine($"Linked {requestedPackage.LibFolderPath} -> {linkTargetPath}");
            return 0;
        }

        private void CreateSymbolicLink(string linkPath, string targetPath)
        {
            SymbolicLink.create(targetPath, linkPath);
        }
    }
}