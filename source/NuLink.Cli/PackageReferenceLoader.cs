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
                _ui.ReportMedium(() => $"Checking package references: {Path.GetFileName(project.ProjectFile.Path)}");

                var packages = LoadPackageReferences(project);
                results.UnionWith(packages);
            }

            return results;
        }
        
        public IEnumerable<PackageReferenceInfo> LoadPackageReferences(ProjectAnalyzer project)
        {
            var referenceLoader = ReferenceLoader.Create(_ui, project);
            return referenceLoader.LoadPackageReferences();
        }

        private abstract class ReferenceLoader
        {
            protected ReferenceLoader(IUserInterface ui, ProjectAnalyzer project, XElement projectXml)
            {
                this.UI = ui;
                this.Project = project;
                this.ProjectXml = projectXml;
            }

            public abstract IEnumerable<PackageReferenceInfo> LoadPackageReferences();

            protected IUserInterface UI { get; }

            protected ProjectAnalyzer Project { get; }

            protected XElement ProjectXml { get; }

            public static ReferenceLoader Create(IUserInterface ui, ProjectAnalyzer project)
            {
                var projectXml = XElement.Load(project.ProjectFile.Path);

                var isSdkStyle = SdkStyleReferenceLoader.IsSdkStyleProject(projectXml);
                var isOldStyle = OldStyleReferenceLoader.IsOldStyleProject(projectXml);

                if (isSdkStyle && !isOldStyle)
                {
                    return new SdkStyleReferenceLoader(ui, project, projectXml);
                }

                if (isOldStyle && !isSdkStyle)
                {
                    return new OldStyleReferenceLoader(ui, project, projectXml);
                }
                
                throw new Exception($"Error: could not recognize project format: {project.ProjectFile.Path}");
            }
        }

        private class SdkStyleReferenceLoader : ReferenceLoader
        {
            public SdkStyleReferenceLoader(IUserInterface ui, ProjectAnalyzer project, XElement projextXml)
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
    
            private string GetPackagesRootFolder()
            {
                var ns = new XmlNamespaceManager(new NameTable());
                ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");
    
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
                return !string.IsNullOrEmpty(sdkValue);
            }
        }

        private class OldStyleReferenceLoader : ReferenceLoader
        {
            public const string MsbuildNamespaceName = "http://schemas.microsoft.com/developer/msbuild/2003";
            
            public OldStyleReferenceLoader(
                IUserInterface ui, ProjectAnalyzer project, XElement projectXml) 
                : base(ui, project, projectXml)
            {
            }

            public override IEnumerable<PackageReferenceInfo> LoadPackageReferences()
            {
                throw new NotImplementedException();
            }

            public static bool IsOldStyleProject(XElement projectXml)
            {
                var defaultNamespace = projectXml.GetDefaultNamespace();
                return (defaultNamespace.NamespaceName == MsbuildNamespaceName);
            }
        }
    }
}