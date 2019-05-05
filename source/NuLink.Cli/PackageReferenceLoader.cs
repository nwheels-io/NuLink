using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Buildalyzer;

namespace NuLink.Cli
{
    public class PackageReferenceLoader
    {
        public IEnumerable<IPackageReferenceInfo> LoadPackageReferences(ProjectAnalyzer project)
        {
            var packages = GetPackageReferences(project).Distinct().ToList();
            var packagesRootFolder = GetPackagesRootFolder(project);

            packages.ForEach(CheckPackageStatus);

            return packages;
                        
            void CheckPackageStatus(PackageReferenceInfo reference)
            {
                reference.PackageFolder = Path.Combine(
                    packagesRootFolder,
                    reference.PackageId.ToLower(),
                    reference.Version.ToLower(),
                    "lib");
            }
        }

        private static IEnumerable<PackageReferenceInfo> GetPackageReferences(ProjectAnalyzer project)
        {
            var csprojXml = XElement.Load(project.ProjectFile.Path);
            var elements = csprojXml.XPathSelectElements("//PackageReference");

            return elements.Select(e => new PackageReferenceInfo {
                PackageId = e.Attribute("Include")?.Value,
                Version = e.Attribute("Version")?.Value
            });
        }

        private static string GetPackagesRootFolder(ProjectAnalyzer project)
        {
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

            var nugetPropsXml = XElement.Load(GetNuGetPropsFilePath(project));
            var result = nugetPropsXml.XPathSelectElement("//msb:NuGetPackageRoot", ns)?.Value;
                
            return result ?? throw new Exception("Could not find NuGetPackageRoot property");
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