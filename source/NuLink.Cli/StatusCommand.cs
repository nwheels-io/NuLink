using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NuLink.Cli
{
    public class StatusCommand : INuLinkCommand
    {
        public int Execute(NuLinkCommandOptions options)
        {
            Console.WriteLine($"Loading solution: {options.ProjectPath}");
            
            var projects = new WorkspaceLoader().LoadProjects(options.ProjectPath, options.ProjectIsSolution);
            //var packages = new PackageReferenceLoader().LoadPackageReferences();
            
            var referenceLoader = new PackageReferenceLoader();
            
            foreach (var project in projects)
            {
                Console.WriteLine($"Checking project: {Path.GetFileName(project.ProjectFile.Path)}");

                var packages = referenceLoader.LoadPackageReferences(project);

                foreach (var package in packages)
                {
                    Console.WriteLine($"{package.PackageId} -> {package.PackageFolder}");
                }

                //var env = project.EnvironmentFactory.GetBuildEnvironment(project.ProjectFile.TargetFrameworks.First());

                //Console.WriteLine(env.MsBuildExePath);

//                foreach (var reference in project.MetadataReferences.OfType<PortableExecutableReference>())
//                {
//                    var isSystem =
//                        reference.Display.Contains("system.", StringComparison.OrdinalIgnoreCase) ||
//                        reference.Display.Contains("microsoft", StringComparison.OrdinalIgnoreCase);
//                    
//                    Console.WriteLine($"Found: {reference.Display} -> {reference.FilePath}");
//                }
            }
            
            return 0;
        }
    }
}