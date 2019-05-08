using System;

namespace NuLink.Cli
{
    public class StatusCommand : INuLinkCommand
    {
        public int Execute(NuLinkCommandOptions options)
        {
            Console.WriteLine(
                $"Checking status of packages in {(options.ProjectIsSolution ? "solution" : "project")}: {options.ConsumerProjectPath}");

            var allProjects = new WorkspaceLoader().LoadProjects(options.ConsumerProjectPath, options.ProjectIsSolution);
            var referenceLoader = new PackageReferenceLoader();
            var allPackages = referenceLoader.LoadPackageReferences(allProjects);

            foreach (var package in allPackages)
            {
                Console.WriteLine();
                Console.WriteLine($"- {package.PackageId} [{package.Version}] @ {package.LibFolderPath}");

                var status = package.CheckStatus();

                Console.WriteLine($"  LibFolderExists ....... {status.LibFolderExists}");
                Console.WriteLine($"  IsLibFolderLinked ..... {status.IsLibFolderLinked}");
                Console.WriteLine($"  LibFolderLinkTargetPath {(status.IsLibFolderLinked ? $"-> {status.LibFolderLinkTargetPath}" : "N/A")}");
                Console.WriteLine($"  LibBackupFolderExists.. {status.LibBackupFolderExists}");
            }

            Console.WriteLine();

            return 0;
        }
    }
}
