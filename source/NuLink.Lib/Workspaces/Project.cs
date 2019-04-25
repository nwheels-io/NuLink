using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NuLink.Lib.MsBuildFormat;
using Semver;

namespace NuLink.Lib.Workspaces
{
    public class Project
    {
        public IEnumerable<PackageReference> GetPackageReferences()
        {
            var elements = Csproj.Xml.XPathSelectElements("//PackageReference");
            
            return elements.Select(e => new PackageReference(
                id: e.Attribute("Include").Value,
                version: SemVersion.Parse(e.Attribute("Version").Value)
            ));
        }
        
        public string Name { get; }
        public CsprojFile Csproj { get; }
        public SemVersion Version { get; }
        
        public bool HasOwnChanges { get; private set; }
        public bool HasDepenencyChanges { get; private set; }
        public bool HasUnpushedCommits { get; private set; }
        public string PendingCommitMessage { get; set; }

        public List<Dependency> DependencyProjects { get; }
        public List<Dependency> DependentProjects { get; }  
    }
}
