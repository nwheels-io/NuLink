using System;
using System.IO;

namespace NuLink.Lib.MsBuildFormat
{
    public class SlnFileParser
    {
        private readonly StreamReader _reader;

        public SlnFileParser(StreamReader reader)
        {
            _reader = reader;
        }

        public SlnFile Parse()
        {
            throw new NotImplementedException();
        }
    }
}