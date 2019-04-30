using System.IO;
using System.Text;
using NuLink.Lib.Abstractions;

namespace NuLink.Lib.MsBuildFormat
{
    public class SlnFilePersistor
    {
        private readonly IImmutableEnvironment _environment;
        private readonly IEnvironmentEffect _effect;

        public SlnFilePersistor(IImmutableEnvironment environment, IEnvironmentEffect effect)
        {
            _environment = environment;
            _effect = effect;
        }

        public void Save(SlnFile sln)
        {
            using (var file = _effect.CreateFile(sln.FileInfo.FullName))
            {
                using (var writer = new StreamWriter(file, Encoding.UTF8))
                {
                    sln.Save(writer);
                    writer.Flush();
                }
            }
        }

        public SlnFile Load(FileInfo fileInfo)
        {
            using (var reader = _environment.OpenTextFile(fileInfo.FullName))
            {
                var sln = new SlnFile(fileInfo);

                var parser = new SlnFileParser(reader);
                parser.Parse(sln);

                return sln;
            }
        }
    }
}