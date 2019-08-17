using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Buildalyzer;

namespace NuLink.Cli.ProjectStyles
{
    public class OldProjectStyle : ProjectStyle
    {
        public const string MsbuildNamespaceName = "http://schemas.microsoft.com/developer/msbuild/2003";
            
        public OldProjectStyle(
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