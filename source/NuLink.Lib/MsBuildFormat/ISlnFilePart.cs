using System.IO;

namespace NuLink.Lib.MsBuildFormat
{
    public interface ISlnFilePart
    {
        void Save(TextWriter writer);
    }
}
