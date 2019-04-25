using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NuLink.Lib.Abstractions
{
    public interface IImmutableEnvironment
    {
        XElement LoadXml(string path);
        StreamReader OpenTextFile(string path);
        string LoadTextFile(string path);
        Task<string> DownloadUrlAsText(string url);
    }
}
