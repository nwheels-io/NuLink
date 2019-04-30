using System.IO;
using System.Text.RegularExpressions;

namespace NuLink.Lib.MsBuildFormat
{
    public class PropertySlnFilePart : ISlnFilePart
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public void Save(TextWriter writer)
        {
            writer.WriteLine($"{Name} = {Value}");
        }

        public static PropertySlnFilePart Parse(Match match)
        {
            return new PropertySlnFilePart {
                Name = match.Groups["PROPERTYNAME"].Value.Trim(),
                Value = match.Groups["PROPERTYVALUE"].Value.Trim()
            };
        }
    }
}