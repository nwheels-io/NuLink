using System;
using System.IO;
using NUnit.Framework;

namespace NuLink.Tests
{
    public static class TestEnvironment
    {
        public static string RepoFolder => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        public static string DemoFolder => Path.Combine(RepoFolder, "demos");
    }
}
