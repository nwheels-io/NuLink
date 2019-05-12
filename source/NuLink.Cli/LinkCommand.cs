using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Murphy.SymbolicLink;

namespace NuLink.Cli
{
    public class LinkCommand : INuLinkCommand
    {
        public int Execute(NuLinkCommandOptions options)
        {
            Console.WriteLine(
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var requestedPackage = GetPackageInfo();
            var status = requestedPackage.CheckStatus();
            var linkTargetPath = Path.Combine(Path.GetDirectoryName(options.LocalProjectPath), "bin", "Debug");

            ValidateOperation();
            PerformOperation();
            
            Console.WriteLine($"Linked {requestedPackage.LibFolderPath} -> {linkTargetPath}");
            return 0;

            PackageReferenceInfo GetPackageInfo()
            {
                var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
                var referenceLoader = new PackageReferenceLoader();
                var allPackages = referenceLoader.LoadPackageReferences(allProjects);
                var package = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);
                return package ?? throw new Exception($"Error: Package not referenced: {options.PackageId}");
            }
            
            void ValidateOperation()
            {
                if (!status.LibFolderExists)
                {
                    throw new Exception($"Error: Cannot link package {options.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
                }

                if (status.IsLibFolderLinked)
                {
                    throw new Exception($"Error: Package {requestedPackage.PackageId} is already linked to {status.LibFolderLinkTargetPath}");
                }

                if (!Directory.Exists(linkTargetPath))
                {
                    throw new Exception($"Error: Target link directory doesn't exist: {linkTargetPath}");
                }
            }

            void PerformOperation()
            {
                if (!status.LibBackupFolderExists)
                {
                    Directory.Move(requestedPackage.LibFolderPath, requestedPackage.LibBackupFolderPath);
                }
                else
                {
                    Console.WriteLine($"Warning: backup folder was not expected to exist: {requestedPackage.LibBackupFolderPath}");
                }

                try
                {
                    CreateSymbolicLink(linkPath: requestedPackage.LibFolderPath, targetPath: linkTargetPath);
                }
                catch
                {
                    RevertOperation();
                    throw;
                }
            }

            void RevertOperation()
            {
                try
                {
                    Console.WriteLine("Failed to create symlink, reverting changes to package folders.");
                    Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"FAILED to revert package changes: {e.Message}");
                    Console.WriteLine($"You have to recover manually!");
                    Console.WriteLine("--- MANUAL RECOVERY INSTRUCTIONS ---");
                    Console.WriteLine($"1. Go to {Path.GetDirectoryName(requestedPackage.LibFolderPath)}");
                    Console.WriteLine($"2. Rename '{Path.GetFileName(requestedPackage.LibBackupFolderPath)}'" +
                                      $" to '{Path.GetFileName(requestedPackage.LibFolderPath)}'");
                    Console.WriteLine("--- END OF RECOVERY INSTRUCTIONS ---");
                }
            }
        }

        private void CreateSymbolicLink(string linkPath, string targetPath)
        {
            SymbolicLink.create(targetPath, linkPath);
        }
    }
}