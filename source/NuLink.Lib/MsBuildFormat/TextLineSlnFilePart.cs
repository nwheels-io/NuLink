using System.IO;
using System.Text.RegularExpressions;

namespace NuLink.Lib.MsBuildFormat
{
    public class TextLineSlnFilePart : ISlnFilePart
    {
        public string Text { get; set; }

        public void Save(TextWriter writer)
        {
            writer.WriteLine(Text);
        }

        public static TextLineSlnFilePart Parse(Match match)
        {
            return new TextLineSlnFilePart {
                Text = match.Value
            };
        }        
    }
}