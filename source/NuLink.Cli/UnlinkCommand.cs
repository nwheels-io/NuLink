using System;
using System.Collections.Generic;
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

            if (options.Mode == NuLinkCommandOptions.LinkMode.All)
            {
                foreach (var package in allPackages)
                {
                    ExecuteForPackage(package);
                }

                return 0;
            }

            var requestedPackage = allPackages.FirstOrDefault(p => p.PackageId == options.PackageId);

            if (requestedPackage == null)
            {
                _ui.ReportError(() => $"Error: Package not referenced: {options.PackageId}");
                return 1;
            }

            return ExecuteForPackage(requestedPackage);
        }

        private int ExecuteForPackage(PackageReferenceInfo requestedPackage)
        {
            var status = requestedPackage.CheckStatus();

            if (!status.LibFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
                return 1;
            }

            if (!status.IsLibFolderLinked)
            {
                _ui.ReportError(() => $"Error: Package {requestedPackage.PackageId} is not linked.");
                return 1;
            }

            if (!status.LibBackupFolderExists)
            {
                _ui.ReportError(() => $"Error: Cannot unlink package {requestedPackage.PackageId}: backup folder not found, expected {requestedPackage.LibBackupFolderPath}");
                return 1;
            }

            Directory.Delete(requestedPackage.LibFolderPath);
            Directory.Move(requestedPackage.LibBackupFolderPath, requestedPackage.LibFolderPath);

            _ui.ReportSuccess(() => $"Unlinked {requestedPackage.PackageId}");
            _ui.ReportSuccess(() => $" {"-X->"} {status.LibFolderLinkTargetPath}", ConsoleColor.Red, ConsoleColor.DarkYellow);
            return 0;
        }
    }
}