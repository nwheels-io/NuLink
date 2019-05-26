using System;
using System.Linq;

namespace NuLink.Cli
{
    public class StatusCommand : INuLinkCommand
    {
        private readonly IUserInterface _ui;

        public StatusCommand(IUserInterface ui)
        {
            _ui = ui;
        }

        public int Execute(NuLinkCommandOptions options)
        {
            _ui.ReportMedium(() =>
                $"Checking status of packages in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader(_ui);
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);
            var orderedPackages = allPackages.OrderBy(p => $"{p.PackageId}@{p.Version}");

            foreach (var package in orderedPackages)
            {
                var status = package.CheckStatus();

                if (status.IsLinkable)
                {
                    PrintPackage(package, status);
                }
            }

            Console.WriteLine();

            return 0;

            void PrintPackage(PackageReferenceInfo reference, PackageStatusInfo status)
            {
                var statusColor = (status.IsCorrupt ? ConsoleColor.Red : ConsoleColor.Green);
                var statusText = (status.IsCorrupt ? "corrupt" : "ok");
                var linkedPath = status.LibFolderLinkTargetPath;
                
                if (status.IsLibFolderLinked)
                {
                    _ui.ReportData(
                        () => $"{reference.PackageId} {reference.Version} {statusText} -> {linkedPath}",
                        ConsoleColor.White, ConsoleColor.Cyan, statusColor, ConsoleColor.Magenta);                    
                }
                else
                {
                    _ui.ReportData(
                        () => $"{reference.PackageId} {reference.Version} {statusText}",
                        ConsoleColor.Gray, ConsoleColor.Cyan, statusColor);                    
                }
            }
        }
    }
}
