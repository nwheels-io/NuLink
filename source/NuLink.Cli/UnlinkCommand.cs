using System;
using System.IO;
using System.Linq;

namespace NuLink.Cli
{
    public class UnlinkCommand : INuLinkCommand
    {
        private readonly IUserInterface _ui;

        public UnlinkCommand(IUserInterface ui)
        {
            _ui = ui;
        }

        public int Execute(NuLinkCommandOptions options)
        {
            _ui.ReportMedium(() =>
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader(_ui);
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);

            var requestedPackage = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);

            if (requestedPackage == null)
            {
                throw new Exception($"Error: Package not referenced: {options.PackageId}");
            }

            var status = requestedPackage.CheckStatus();

            if (!status.LibFolderExists)
            {
                throw new Exception($"Error: Cannot unlink package {options.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
            }

            if (!status.IsLibFolderLinked)
            {
                throw new Exception($"Error: Package {requestedPackage.PackageId} is not linked.");
            }

            if (!status.LibBackupFolderExists)
            {
                throw new Exception($"Error: Cannot unlink package {options.PackageId}: backup folder not found, expected {requestedPackage.LibBackupFolderPath}");
            }

            Directory.Delete(requestedPackage.LibFolderPath);
            Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);

            _ui.ReportSuccess(() => $"Unlinked {requestedPackage.PackageId}");
            _ui.ReportSuccess(() => $" {"-X->"} {status.LibFolderLinkTargetPath}", ConsoleColor.Red, ConsoleColor.DarkYellow);
            return 0;
        }
    }
}