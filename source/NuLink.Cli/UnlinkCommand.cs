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

        public void Execute(NuLinkCommandOptions options)
        {
            _ui.ReportMedium(() =>
                $"Checking package references in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader(_ui);
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);

            if (options.PackageId == null)
            {
                foreach (var package in allPackages)
                {
                    UnlinkPackage(package, true);
                }

                return;
            }

            var requestedPackage = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);

            if (requestedPackage == null)
            {
                _ui.ReportError(() => $"Error: Package not referenced: {options.PackageId}");
                return;
            }

            UnlinkPackage(requestedPackage, false);
        }

        private void UnlinkPackage(PackageReferenceInfo requestedPackage, bool allPackages)
        {
            var status = requestedPackage.CheckStatus();

            if (!status.LibFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
                return;
            }

            if (!status.IsLibFolderLinked)
            {
                _ui.Report(allPackages ? VerbosityLevel.Low : VerbosityLevel.Error, () =>
                    $"{(allPackages ? string.Empty : string.Concat(VerbosityLevel.Error.ToString(), ": "))}" +
                    $"Package {requestedPackage.PackageId} is not linked.");

                return;
            }

            if (!status.LibBackupFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: backup folder not found, expected {requestedPackage.LibBackupFolderPath}");
                return;
            }

            Directory.Delete(requestedPackage.LibFolderPath);
            Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);

            _ui.ReportSuccess(() => $"Unlinked {requestedPackage.PackageId}");
            _ui.ReportSuccess(() => $" {"-X->"} {status.LibFolderLinkTargetPath}", ConsoleColor.Red, ConsoleColor.DarkYellow);
        }
    }
}