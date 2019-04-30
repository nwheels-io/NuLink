using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NuLink.Lib.Common;

namespace NuLink.Lib.MsBuildFormat
{
    public class GlobalSlnFilePart : ISlnFilePart
    {
        public void Save(TextWriter writer)
        {
            writer.WriteLine("Global");
            Sections.ForEach(s => s.Save(writer));
            writer.WriteLine("EndGlobal");
        }
        
        public List<SlnSection> Sections { get; } = new List<SlnSection>();
    }

    public class SlnSection
    {
        public string Name { get; set; }
        public SlnSectionKind Kind { get; set; }
        public List<PropertySlnFilePart> Properties { get; } = new List<PropertySlnFilePart>();

        public void Save(TextWriter writer)
        {
            var kindText = Kind.ToString().ToCamelCase();
            writer.WriteLine($"\tGlobalSection({Name}) = {kindText}");

            Properties.ForEach(p => {
                writer.Write("\t\t");
                p.Save(writer);
            });

            writer.WriteLine("\tEndGlobalSection");
        }

        public static SlnSection Parse(Match match)
        {
            return new SlnSection {
                Name = match.Groups["SECTIONNAME"].Value.Trim(),
                Kind = match.Groups["SECTIONKIND"].Value.Trim().ToEnum<SlnSectionKind>()
            };
        }        
    }
    
    public enum SlnSectionKind
    {
        PreSolution,
        PostSolution        
    }
}
