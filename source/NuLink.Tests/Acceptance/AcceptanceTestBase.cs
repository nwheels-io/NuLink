using System;
using System.IO;
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
                if (package.State.HasFlag(PackageStates.Linked))
                {
                    Exec(
                        "nulink", "link", 
                        "-c", Path.Combine(TestEnvironment.DemoFolder, "NuLink.TestCase.Consumer"),
                        "-p", packageId);
                }

                if (package.State.HasFlag(PackageStates.Patched))
                {
                    var packageSourceFolder = Path.Combine(TestEnvironment.DemoFolder, packageId);
                    var patchFilePath = Path.Combine(TestEnvironment.DemoFolder, "modify-test-case-packages.patch");
                    
                    ExecIn(packageSourceFolder, "git", "apply", patchFilePath);
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
                var libBackupFolderExists = Directory.Exists(Path.Combine(packageFolderPath, "nulink-backup.lib"));

                isLinked.ShouldBe(package.State.HasFlag(PackageStates.Linked));
                libBackupFolderExists.ShouldBe(package.State.HasFlag(PackageStates.Linked));
            }
        }
    }
}
