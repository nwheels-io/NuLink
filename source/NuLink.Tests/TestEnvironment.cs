using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace NuLink.Tests
{
    public static class TestEnvironment
    {
        #if DEBUG
        private const string ConfigurationName = "Debug";
        #else
        private const string ConfigurationName = "Release";
        #endif

        public static readonly string ExecutableFileExtension = (
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ".exe"
                : "");

        public static string RepoFolder => Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        public static string DemoFolder => Path.Combine(RepoFolder, "demos");
        public static string CompiledNuLinkBinaryPath => Path.Combine(
            RepoFolder,
            "source",
            "NuLink.Cli",
            "bin",
            ConfigurationName,
            "netcoreapp2.1",
            "nulink.dll");
        public static string InstalledNuLinkBinaryPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotnet",
            "tools",
            "nulink" + ExecutableFileExtension);
        public static bool ShouldUseInstalledNuLinkBinary =>
            Environment.GetEnvironmentVariable("NULINK_TEST_USE_INSTALLED") == "YES";
    }
}
