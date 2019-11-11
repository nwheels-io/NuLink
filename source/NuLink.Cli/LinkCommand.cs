using System;
using System.Collections.Generic;
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

            var allPackages = GetAllPackages(options);

            IEnumerable<string> allProjectsInRoot;
            PackageReferenceInfo requestedPackage = null;
            PackageStatusInfo status = null;
            string localProjectPath = options.LocalProjectPath;

            switch (options.Mode)
            {
                case NuLinkCommandOptions.LinkMode.Single:
                    requestedPackage = GetPackage(allPackages, options.PackageId);
                    status = requestedPackage.CheckStatus();
                    break;

                case NuLinkCommandOptions.LinkMode.PackageToAll:
                    requestedPackage = GetPackage(allPackages, options.PackageId);
                    status = requestedPackage.CheckStatus();
                    allProjectsInRoot = GetAllProjects(options.RootDirectory);
                    localProjectPath = allProjectsInRoot.FirstOrDefault(proj => proj.Contains(requestedPackage.PackageId));
                    break;

                case NuLinkCommandOptions.LinkMode.All:
                    allProjectsInRoot = GetAllProjects(options.RootDirectory);

                    foreach (var package in allPackages)
                    {
                        requestedPackage = package;
                        status = requestedPackage.CheckStatus();
                        localProjectPath = allProjectsInRoot.FirstOrDefault(proj => proj.Contains(requestedPackage.PackageId));
                        ExecuteForPackage(requestedPackage, status, options, localProjectPath);
                    }

                    return 0;
            }

            return ExecuteForPackage(requestedPackage, status, options, localProjectPath);
        }

        private int ExecuteForPackage(PackageReferenceInfo requestedPackage, PackageStatusInfo status, NuLinkCommandOptions options, string localProjectPath)
        {
            if (localProjectPath == null)
            {
                _ui.ReportError(() => $"Error: Cannot find corresponding project to package {requestedPackage.PackageId}");
                return 1;
            }

            var linkTargetPath = Path.Combine(Path.GetDirectoryName(localProjectPath), "bin", "Debug");

            if (!ValidateOperation())
            {
                return 1;
            }

            PerformOperation();

            _ui.ReportSuccess(() => $"Linked {requestedPackage.LibFolderPath}");
            _ui.ReportSuccess(() => $" -> {linkTargetPath}", ConsoleColor.Magenta);
            return 0;

            bool ValidateOperation()
            {
                if (!status.LibFolderExists)
                {
                    _ui.ReportError(() => $"Error: Cannot link package {options.PackageId}: 'lib' folder not found, expected {requestedPackage.LibFolderPath}");
                    return false;
                }

                if (status.IsLibFolderLinked)
                {
                    _ui.ReportError(() => $"Error: Package {requestedPackage.PackageId} is already linked to {status.LibFolderLinkTargetPath}");
                    return false;
                }

                if (!Directory.Exists(linkTargetPath))
                {
                    _ui.ReportError(() => $"Error: Target link directory doesn't exist: {linkTargetPath}");
                    return false;
                }

                return true;
            }

            void PerformOperation()
            {
                if (!status.LibBackupFolderExists)
                {
                    Directory.Move(requestedPackage.LibFolderPath, requestedPackage.LibBackupFolderPath);
                }
                else
                {
                    _ui.ReportWarning(() =>
                        $"Warning: backup folder was not expected to exist: {requestedPackage.LibBackupFolderPath}");
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

        private PackageReferenceInfo GetPackage(HashSet<PackageReferenceInfo> allPackages, string packageId)
        {
            var package = allPackages.FirstOrDefault(p => p.PackageId == packageId);
            return package ?? throw new Exception($"Error: Package not referenced: {packageId}");
        }

        private HashSet<PackageReferenceInfo> GetAllPackages(NuLinkCommandOptions options)
        {
            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader(_ui);
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);
            return allPackages;
        }

        private IEnumerable<string> GetAllProjects(string rootDir)
        {
            var slnPaths = Directory.GetFiles(rootDir, "*.sln", SearchOption.AllDirectories);
            var allProjects = new WorkspaceLoader().LoadProjects(slnPaths).Select(proj => proj.ProjectFile.Path);
            return allProjects;
        }
    }
}