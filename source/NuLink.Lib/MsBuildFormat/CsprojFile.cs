using System.Xml.Linq;

namespace NuLink.Lib.MsBuildFormat
{
    public class CsprojFile
    {
        public CsprojFile(string filePath, XElement xml)
        {
            FilePath = filePath;
            Xml = xml;
        }

        public string FilePath { get; }
        public XElement Xml { get; }

        public static CsprojFile ReadFromFile(string path)
        {
            var xml = XElement.Load(path);
            return new CsprojFile(path, xml);
        }
    }
}
