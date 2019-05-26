using System;
using System.Collections.Generic;
using System.Threading;

namespace NuLink.Tests.Acceptance
{
    public class AcceptanceTestCase
    {
        public string GivenCurrentDiectory = TestEnvironment.DemoFolder;
        public readonly DemoSolutionState Given = new DemoSolutionState();
        public Action When;
        public readonly DemoSolutionState Then = new DemoSolutionState();
    }

    public class DemoSolutionState
    {
        public readonly Dictionary<string, PackageEntry> Packages = new Dictionary<string, PackageEntry>();     
        public readonly Dictionary<string, string> ExpectedValues = new Dictionary<string, string>();
        public IReadOnlyList<string> ExpectedNuLinkOutput = null;
    }

    public class PackageEntry
    {
        public PackageEntry(string version, PackageStates state)
        {
            Version = version;
            State = state;
        }

        public string Version;
        public PackageStates State;
    }
    
    [Flags]
    public enum PackageStates
    {
        Original = 0,
        Patched = 0x01,
        Built = 0x02,
        Linked = 0x04,
        PatchedAndBuilt = Patched | Built,
        PatchedAndLinked = Patched | Linked,
        BuiltAndLinked = Built | Linked,
        PatchedBuiltAndLinked = Patched | Built | Linked
    }
}
