using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Buildalyzer;

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
                _ui.ReportLow(() => $"Checking package references: {Path.GetFileName(project.ProjectFile.Path)}");

                var packages = LoadPackageReferences(project);
                results.UnionWith(packages);
            }

            return results;
        }
        
        public IEnumerable<PackageReferenceInfo> LoadPackageReferences(ProjectAnalyzer project)
        {
            var packagesRootFolder = GetPackagesRootFolder(project);
            var packages = GetPackages();

            return packages.Where(p => p != null);
                        
            IEnumerable<PackageReferenceInfo> GetPackages()
            {
                var csprojXml = XElement.Load(project.ProjectFile.Path);
                var elements = csprojXml.XPathSelectElements("//PackageReference");
            
                return elements.Select(e => {
                    var packageId = e.Attribute("Include")?.Value;
                    var version = e.Attribute("Version")?.Value;

                    if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(version))
                    {
                        var folder = GetPackageFolder(packageId, version);
                        return new PackageReferenceInfo(packageId, version, folder);
                    }

                    return null;
                });
            }

            string GetPackageFolder(string packageId, string version)
            {
                var packageFolderPath = Path.Combine(
                    packagesRootFolder,
                    packageId.ToLower(),
                    version.ToLower());

                return packageFolderPath;
            }
        }

        private string GetPackagesRootFolder(ProjectAnalyzer project)
        {
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

            var nugetPropsXml = XElement.Load(GetNuGetPropsFilePath(project));
            var result = 
                nugetPropsXml.XPathSelectElement("//msb:NuGetPackageRoot", ns)?.Value
                ?? throw new Exception("Could not find NuGetPackageRoot property");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var userProfilePath = Environment.GetEnvironmentVariable("UserProfile");
                _ui.ReportLow(() => $"Detected Windows: $(UserProfile)=[{userProfilePath}]");
                result = result.Replace("$(UserProfile)", userProfilePath, StringComparison.InvariantCultureIgnoreCase);
            }
            
            return result;
        }

        private static string GetNuGetPropsFilePath(ProjectAnalyzer project)
        {
            var filePath = Path.Combine(
                Path.GetDirectoryName(project.ProjectFile.Path),
                "obj",
                $"{Path.GetFileName(project.ProjectFile.Path)}.nuget.g.props");

            return filePath;
        }
    }
}