using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Buildalyzer;

namespace NuLink.Cli.ProjectStyles
{
    public class OldProjectStyle : ProjectStyle
    {
        public OldProjectStyle(
            IUserInterface ui, ProjectAnalyzer project, XElement projectXml) 
            : base(ui, project, projectXml)
        {
        }

        public override IEnumerable<PackageReferenceInfo> LoadPackageReferences()
        {
            var projectDirectory = Path.GetDirectoryName(Project.ProjectFile.Path);
            var allHintPaths = FindReferenceHintPaths();
            
            var packages = LoadPackagesConfig()
                .Select(EnrichWithPackageRootPath)
                .Where(info => info != null)
                .ToArray();

            return packages;

            PackageReferenceInfo EnrichWithPackageRootPath(PackageReferenceInfo package)
            {
                var hintPath = allHintPaths.FirstOrDefault(hint => IsAssemblyFromPackage(hint, package));

                if (hintPath != null)
                {
                    var relativePathWithoutDll = Path.Combine(hintPath.Parts.SkipLast(1).ToArray());
                    var absolutePath = Path.GetFullPath(
                        relativePathWithoutDll,
                        basePath: projectDirectory);
                    
                    UI.ReportHigh(() => $"OLD-STYLE-PKG-REF: {package.PackageId + "@" + package.Version} -> {absolutePath}");
                    
                    return new PackageReferenceInfo(
                        package.PackageId, package.Version, rootFolderPath: absolutePath);
                }
                
                return null;
            }
        }

        private IEnumerable<ParsedPath> FindReferenceHintPaths()
        {
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("msb", MsbuildNamespaceUri);
         
            var hintPathElements = ProjectXml.XPathSelectElements("//msb:HintPath", ns);
            return hintPathElements
                .Select(element => new ParsedPath(element.Value))
                .Where(parsed => parsed.Parts.Count > 0);
        }
        
        private IEnumerable<PackageReferenceInfo> LoadPackagesConfig()
        {
            var packagesConfigFilePath = Path.Combine(
                Path.GetDirectoryName(Project.ProjectFile.Path),
                "packages.config");

            UI.ReportLow(() => $"Loading packages config: {packagesConfigFilePath}");
            
            var packagesConfigXml = XElement.Load(packagesConfigFilePath);
            var packageElements = packagesConfigXml.XPathSelectElements("//package");
            var packageConfigs = packageElements.Select(CreateEntry).ToArray();
            
            UI.ReportLow(() => $"Loaded {packageConfigs.Length} package(s)");
            return packageConfigs;

            PackageReferenceInfo CreateEntry(XElement element)
            {
                var id = element.Attribute("id")?.Value;
                var version = element.Attribute("version")?.Value;

                if (string.IsNullOrWhiteSpace("id") || string.IsNullOrWhiteSpace(version))
                {
                    throw new Exception(
                        "Error in packages.config: 'package' element has no 'id' or 'version' attribute");
                }

                return new PackageReferenceInfo(id, version, rootFolderPath: string.Empty);
            }
        }

        private bool IsAssemblyFromPackage(ParsedPath pathHint, PackageReferenceInfo package)
        {
            var ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            var partText = $"{package.PackageId}.{package.Version}";
            var partIndex = pathHint.Parts
                .TakeWhile(p => !p.Equals(partText, ignoreCase))
                .Count();

            if (partIndex > 0 && partIndex < pathHint.Parts.Count - 1)
            {
                return (
                    pathHint.Parts[partIndex - 1].Equals("packages", ignoreCase) &&
                    pathHint.Parts[partIndex + 1].Equals("lib", ignoreCase));
            }

            return false;
        }

        public static bool IsOldStyleProject(XElement projectXml)
        {
            var defaultNamespace = projectXml.GetDefaultNamespace();
            return (defaultNamespace.NamespaceName == MsbuildNamespaceUri);
        }

        private class ParsedPath
        {
            public ParsedPath(string path)
            {
                Parts = path.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);
            }

            public readonly IReadOnlyList<string> Parts;
        }
    }
}