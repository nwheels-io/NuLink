using System;
using System.Collections.Generic;
using System.IO;

namespace NuLink.Lib
{
    public class CommandOptions
    {
        public CommandOptions(
            FileInfo solution, FileInfo configuration, IReadOnlyList<string> packageNames, bool dryRun)
        {
            Solution = solution;
            Configuration = configuration;
            PackageNames = packageNames;
            DryRun = dryRun;
        }

        public FileInfo Solution { get; }
        public FileInfo Configuration { get; }
        public IReadOnlyList<string> PackageNames { get; }
        public bool DryRun { get; }
    }
}
