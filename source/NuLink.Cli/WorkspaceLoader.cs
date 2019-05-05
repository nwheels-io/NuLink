using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;

namespace NuLink.Cli
{
    public class WorkspaceLoader
    {
        public IEnumerable<ProjectAnalyzer> LoadProjects(string filePath, bool isSolution)
        {
            var analyzerManager = (isSolution 
                ? new AnalyzerManager(filePath) 
                : new AnalyzerManager());

            if (isSolution)
            {
                return analyzerManager.Projects.Values;
            }
            else
            {
                return new[] { analyzerManager.GetProject(filePath) };
            }
        }

//        public IEnumerable<IPackageReferenceInfo> LoadPackageReferences(
//            IEnumerable<ProjectAnalyzer> projects, 
//            string packageIdOrNull,
//            bool recursive = false,
//            bool includeFrameworkPackages = false)
//        {
//            var projectList = projects.ToList();
//            var resolver = new PublishAssembliesResolver();
//            var resolutionResults = resolver.Resolve(projectList);
//
//            
//            return resolutionResults.Select(result => new PackageReferenceInfo {
//                PackageId = Path.GetFileNameWithoutExtension(result.AssemblyFile),
//                Version = "0.0.0",
//                PackageFolder = Path.GetDirectoryName(result.FullPath),
//                IsLinked = false,
//                AssemblyFiles = new List<string> { result.AssemblyFile } 
//            });
//        }
    }
}