using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Buildalyzer;

namespace NuLink.Cli.ProjectStyles
{
    public class SdkProjectStyle : ProjectStyle
    {
        public SdkProjectStyle(IUserInterface ui, ProjectAnalyzer project, XElement projextXml)
            : base(ui, project, projextXml)
        {
        }

        public override IEnumerable<PackageReferenceInfo> LoadPackageReferences()
        {
            var packagesRootFolder = GetPackagesRootFolder();
            var packages = GetPackages();
    
            return packages.Where(p => p != null);
                            
            IEnumerable<PackageReferenceInfo> GetPackages()
            {
                var elements = ProjectXml.XPathSelectElements("//PackageReference");
                
                return elements.Select(e => {
                    var packageId = e.Attribute("Include")?.Value;
                    var version = e.Attribute("Version")?.Value;
    
                    if (!String.IsNullOrWhiteSpace(packageId) && !String.IsNullOrWhiteSpace(version))
                    {
                        var folder = GetPackageFolder(packageId, version);
                        return new PackageReferenceInfo(
                            packageId, 
                            version, 
                            rootFolderPath: folder,
                            libSubfolderPath: "lib");
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
    
        private string GetPackagesRootFolder()
        {
            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("msb", MsbuildNamespaceUri);
    
            var nugetPropsXml = XElement.Load(GetNuGetPropsFilePath());
            var result = 
                nugetPropsXml.XPathSelectElement("//msb:NuGetPackageRoot", ns)?.Value
                ?? throw new Exception("Could not find NuGetPackageRoot property");
    
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var userProfilePath = Environment.GetEnvironmentVariable("UserProfile");
                UI.ReportLow(() => $"Detected Windows: $(UserProfile)=[{userProfilePath}]");
                result = result.Replace("$(UserProfile)", userProfilePath, StringComparison.InvariantCultureIgnoreCase);
            }
                
            return result;
        }
    
        private string GetNuGetPropsFilePath()
        {
            var filePath = Path.Combine(
                Path.GetDirectoryName(Project.ProjectFile.Path),
                "obj",
                $"{Path.GetFileName(Project.ProjectFile.Path)}.nuget.g.props");
    
            return filePath;
        }

        public static bool IsSdkStyleProject(XElement projectXml)
        {
            var sdkValue = projectXml.Attribute("Sdk")?.Value;
            return !String.IsNullOrEmpty(sdkValue);
        }
    }
}