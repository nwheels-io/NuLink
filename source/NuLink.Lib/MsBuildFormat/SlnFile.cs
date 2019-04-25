using System.IO;

namespace NuLink.Lib.MsBuildFormat
{
    public class SlnFile
    {
        
        

        public static SlnFile ReadFromFile(string path)
        {
            using (var reader = File.OpenText(path))
            {
                var parser = new SlnFileParser(reader);
                return parser.Parse();
            }
        }
    }
}
