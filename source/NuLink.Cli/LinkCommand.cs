using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Murphy.SymbolicLink;

namespace NuLink.Cli
{
    public class LinkCommand : INuLinkCommand
    {
        private readonly IUserInterface _ui;

        public LinkCommand(IUserInterface ui)
        {
            _ui = ui;
        }

        public int Execute(NuLinkCommandOptions options)
        {
            _ui.ReportMedium(() =>
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var requestedPackage = GetPackageInfo();
            var status = requestedPackage.CheckStatus();
            var linkTargetPath = Path.Combine(Path.GetDirectoryName(options.LocalProjectPath), "bin", "Debug");

            ValidateOperation();
            PerformOperation();
            
            _ui.ReportSuccess(() => $"Linked {requestedPackage.LibFolderPath}");
            _ui.ReportSuccess(() => $" -> {linkTargetPath}", ConsoleColor.Magenta);
            return 0;

            PackageReferenceInfo GetPackageInfo()
            {
                var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
                var referenceLoader = new PackageReferenceLoader(_ui);
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
                    _ui.ReportWarning(() => $"Warning: backup folder was not expected to exist: {requestedPackage.LibBackupFolderPath}");
                }

                try
                {
                    SymbolicLinkWithDiagnostics.Create(
                        fromPath: requestedPackage.LibFolderPath, 
                        toPath: linkTargetPath);
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
                    _ui.ReportError(() => "Failed to create symlink, reverting changes to package folders.");
                    Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);
                }
                catch (Exception e)
                {
                    _ui.ReportError(() => $"FAILED to revert package changes: {e.Message}");
                    _ui.ReportError(() => $"You have to recover manually!");
                    _ui.ReportError(() => "--- MANUAL RECOVERY INSTRUCTIONS ---");
                    _ui.ReportError(() => $"1. Go to {Path.GetDirectoryName(requestedPackage.LibFolderPath)}");
                    _ui.ReportError(() => $"2. Rename '{Path.GetFileName(requestedPackage.LibBackupFolderPath)}'" +
                                      $" to '{Path.GetFileName(requestedPackage.LibFolderPath)}'");
                    _ui.ReportError(() => "--- END OF RECOVERY INSTRUCTIONS ---");
                }
            }
        }
    }
}