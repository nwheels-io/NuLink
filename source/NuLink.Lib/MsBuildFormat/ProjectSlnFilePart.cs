using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using NuLink.Lib.Common;

namespace NuLink.Lib.MsBuildFormat
{
    public class ProjectSlnFilePart : ISlnFilePart
    {
        public string TypeGuid { get; set; }
        public string ProjectGuid { get; set; }
        public string Name { get; set; }
        public string SolutionRelativePath { get; set; }

        public bool IsCSharpProject()
        {
            return (TypeGuid == ProjectTypeGuids.CSharpNetCore || TypeGuid == ProjectTypeGuids.CSharpNetFx);
        }

        public bool IsSolutionFolder()
        {
            return (TypeGuid == ProjectTypeGuids.Folder);
        }

        public void Save(TextWriter writer)
        {
            writer.WriteLine($"Project(\"{TypeGuid}\") = \"{Name}\", \"{SolutionRelativePath}\", \"{ProjectGuid}\"");
            writer.WriteLine("EndProject");
        }

        public string GetConsumerRelativePath(SlnFile solution, ProjectSlnFilePart consumerProject)
        {
            var thisAbsolutePath = solution.GetProjectFileInfo(this);
            var consumerAbsolutePath = solution.GetProjectFileInfo(consumerProject);

            var thisPathRelativeToConsumer = thisAbsolutePath.GetPathRelativeTo(
                relativeTo: consumerAbsolutePath);

            return thisPathRelativeToConsumer;
        }

        public static ProjectSlnFilePart Parse(Match match)
        {
            return new ProjectSlnFilePart {
                TypeGuid = match.Groups["PROJECTTYPEGUID"].Value.Trim(),
                Name = match.Groups["PROJECTNAME"].Value.Trim(),
                SolutionRelativePath = match.Groups["RELATIVEPATH"].Value.Trim(),
                ProjectGuid = match.Groups["PROJECTGUID"].Value.Trim()
            };
        }

        public static XElement TryFindProjectVersionElement(XElement projectXml)
        {
            var versionElement = ((System.Collections.IEnumerable) projectXml.XPathEvaluate("PropertyGroup/Version"))
                .OfType<XElement>()
                .FirstOrDefault();

            return versionElement;
        }

        public static XElement AddProjectVersionElement(XElement projectXml, string version)
        {
            var newl = Environment.NewLine;
            var indent = "  ";
            var versionElement = new XElement("Version", version);

            projectXml.Add(
                newl, indent,
                new XElement("PropertyGroup",
                    newl, indent, indent,
                    versionElement,
                    newl, indent
                ),
                newl, newl
            );

            return versionElement;
        }        
    }
}