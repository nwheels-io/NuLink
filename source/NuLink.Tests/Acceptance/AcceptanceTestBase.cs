using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Murphy.SymbolicLink;
using NUnit.Framework;
using Shouldly;

namespace NuLink.Tests.Acceptance
{
    public class AcceptanceTestBase
    {
        protected void ExecuteTestCase(AcceptanceTestCase testCase)
        {
            Cleanup(testCase);
            SetupGiven(testCase);
            testCase.When?.Invoke();
            VerifyThen(testCase);
        }
        
        protected void Exec(string program, params string[] args)
        {
            ExecIn(TestEnvironment.RepoFolder, program, args);
        }

        protected void ExecIn(string directory, string program, params string[] args)
        {
            var exitCode = ExternalProgram.Execute(
                out var output,
                nameOrFilePath: program,
                args: args,
                workingDirectory: directory,
                validateExitCode: false);

            if (exitCode != 0)
            {
                Console.WriteLine($"PROGRAM FAILED: {program} {string.Join(" ", args)}");
                Console.WriteLine($"--- PROGRAM OUTPUT ---");
                foreach (var line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine($"--- END OF PROGRAM OUTPUT ---");
                throw new Exception($"Program '{program}' failed with code {exitCode}.");
            }
        }

        protected void ExecNuLinkIn(string directory, params string[] args)
        {
//            var exePath = Environment.GetEnvironmentVariable("NULINK_TEST_USE_INSTALLED");
//
//        public static string NuLinkProgramPath =>
//            Environment.GetEnvironmentVariable("NULINK_TEST_PROGRAM_PATH")
//            ?? CompiledNuLinkBinaryPath;

            if (TestEnvironment.ShouldUseInstalledNuLinkBinary)
            {
                ExecIn(directory, TestEnvironment.InstalledNuLinkBinaryPath, args);
            }
            else
            {
                ExecIn(
                    directory, 
                    "dotnet",
                    new[] { TestEnvironment.CompiledNuLinkBinaryPath }.Concat(args).ToArray());
            }
        }
        
        protected string ConsumerSolutionFolder => Path.Combine(TestEnvironment.DemoFolder, "NuLink.TestCase.Consumer");
        protected string ConsumerSolutionFile => Path.Combine(ConsumerSolutionFolder, "NuLink.TestCase.Consumer.sln");
        protected string PackageProjectFolder(string packageId) => Path.Combine(
            TestEnvironment.DemoFolder, 
            packageId,
            packageId);
        protected string PackageProjectFile(string packageId) =>
            Path.Combine(PackageProjectFolder(packageId), $"{packageId}.csproj");
        protected string PackagesRootFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");
        protected string PackageNugetFolder(string packageId) => Path.Combine(
            PackagesRootFolder,
            packageId.ToLower());
        protected string PackageNugetLibFolder(string packageId, string version) => Path.Combine(
            PackageNugetFolder(packageId),
            version,
            "lib");

        private void Cleanup(AcceptanceTestCase testCase)
        {
            ExecIn(TestEnvironment.DemoFolder, "git", "clean", "-dfx");
            ExecIn(TestEnvironment.DemoFolder, "git", "checkout", ".");

            foreach (var package in testCase.Given.Packages)
            {
                Directory.Delete(PackageNugetFolder(package.Key), recursive: true);
            }
            
            ExecIn(ConsumerSolutionFolder, "dotnet", "restore");
        }
        
        private void SetupGiven(AcceptanceTestCase testCase)
        {
            foreach (var package in testCase.Given.Packages)
            {
                SetupGivenPackageState(package.Key, package.Value);
            }

            Directory.SetCurrentDirectory(testCase.GivenCurrentDiectory ?? TestEnvironment.DemoFolder);
            
            void SetupGivenPackageState(string packageId, PackageEntry package)
            {
                var packageSourceFolder = Path.Combine(TestEnvironment.DemoFolder, packageId);
                var patchFilePath = Path.Combine(TestEnvironment.DemoFolder, "modify-test-case-packages.patch");

                if (package.State.HasFlag(PackageStates.Patched))
                {
                    ExecIn(packageSourceFolder, "git", "apply", patchFilePath);
                }

                if (package.State.HasFlag(PackageStates.Built))
                {
                    ExecIn(packageSourceFolder, "dotnet", "build", "-c", "Debug");
                }

                if (package.State.HasFlag(PackageStates.Linked))
                {
                    ExecNuLinkIn(
                        ConsumerSolutionFolder,
                        "link", 
                        "-p", packageId,
                        "-l", PackageProjectFile(packageId));
                }
            }
        }

        private void VerifyThen(AcceptanceTestCase testCase)
        {
            foreach (var package in testCase.Then.Packages)
            {
                VerifyThenPackageState(package.Key, package.Value);
            }

            foreach (var expected in testCase.Then.ExpectedValues)
            {
                Environment.SetEnvironmentVariable($"TEST_{expected.Key}", expected.Value);
            }
            
            Exec("dotnet", "test", Path.Combine(ConsumerSolutionFolder, "NuLink.TestCase.ConsumerLib.Tests"));
            
            void VerifyThenPackageState(string packageId, PackageEntry package)
            {
                var packageFolderPath = PackageNugetFolder(packageId);
                var libFolderPath = PackageNugetLibFolder(packageId, package.Version);
                var libFolderTargetPath = SymbolicLink.resolve(libFolderPath);
                var isLinked = (libFolderTargetPath != null && libFolderTargetPath != libFolderPath);
                var libBackupFolderExists = Directory.Exists(Path.Combine(packageFolderPath, package.Version, "nulink-backup.lib"));

                isLinked.ShouldBe(package.State.HasFlag(PackageStates.Linked));
                libBackupFolderExists.ShouldBe(package.State.HasFlag(PackageStates.Linked));
            }
        }
    }
}
