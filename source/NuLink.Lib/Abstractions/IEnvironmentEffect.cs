using System.IO;
using System.Xml.Linq;

namespace NuLink.Lib.Abstractions
{
    public interface IEnvironmentEffect
    {
        Stream CreateFile(string path);
        void CreateDirectory(string path);
        void SaveXml(XElement xml, string path);
        void SaveTextFile(string contents, string ath);
        void MoveFile(string fromPath, string toPath);
        void MoveFolder(string fromPath, string toPath);
        void DeleteFile(string path);
        void DeleteFolder(string path, bool recursiveForce);
        void CommitSource(ISourceRepository repo, CodeAuthor author, string message);
    }
}
