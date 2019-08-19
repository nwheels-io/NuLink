using System.Collections.Generic;
using System.IO;
using Buildalyzer;
using NuLink.Cli.ProjectStyles;

namespace NuLink.Cli
{
    public class PackageReferenceLoader
    {
        private readonly IUserInterface _ui;

        public PackageReferenceLoader(IUserInterface ui)
        {
            _ui = ui;
        }

        public HashSet<PackageReferenceInfo> LoadPackageReferences(IEnumerable<ProjectAnalyzer> projects)
        {
            var results = new HashSet<PackageReferenceInfo>();

            foreach (var project in projects)
            {
                _ui.ReportMedium(() => $"Checking package references: {Path.GetFileName(project.ProjectFile.Path)}");

                var projectStyle = ProjectStyle.Create(_ui, project);
                var projectPackages = projectStyle.LoadPackageReferences();
                
                results.UnionWith(projectPackages);
            }

            return results;
        }
        
    }
}