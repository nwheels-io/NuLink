using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NuLink.Lib.Workspaces;
using Semver;

namespace NuLink.Lib.MsBuildFormat
{
    public class CsprojFile
    {
        public CsprojFile(FileInfo fileInfo, XElement xml)
        {
            FileInfo = fileInfo;
            Xml = xml;
        }

        public IEnumerable<PackageReference> GetPackageReferences()
        {
            var elements = Xml.XPathSelectElements("//PackageReference");
            
            return elements.Select(e => new PackageReference(
                id: e.Attribute("Include").Value,
                version: SemVersion.Parse(e.Attribute("Version").Value)
            ));
        }
        
        public FileInfo FileInfo { get; }
        public XElement Xml { get; }
    }
}
