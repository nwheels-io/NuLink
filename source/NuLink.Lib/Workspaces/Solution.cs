using System.Collections.Generic;
using NuLink.Lib.MsBuildFormat;

namespace NuLink.Lib.Workspaces
{
    public class Solution
    {
        public Solution(SlnFile sln)
        {
            this.Sln = sln;
        }
        
        public SlnFile Sln { get; }
        public IReadOnlyList<Project> Projects { get; }

        public IReadOnlyDictionary<string, Project> ProjectByName { get; }
    }
}

