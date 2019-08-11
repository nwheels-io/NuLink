using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Murphy.SymbolicLink;
using NUnit.Framework;
using Shouldly;

namespace NuLink.Tests.Acceptance
{
    public class AcceptanceTestBase
    {
        [ThreadStatic]
        private static List<string> NuLinkOutput;

        public static readonly IReadOnlyList<AcceptanceTestTarget> AllTargets; 

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ExternalProgram.ExecIn(TestEnvironment.DemoFolder, "git", "checkout", ".");
        }

        protected void ExecuteTestCase(AcceptanceTestCase testCase)
        {
            try
            {
                Cleanup(testCase);
                SetupGiven(testCase);
                NuLinkOutput = new List<string>();
            
                testCase.When?.Invoke();
            
                VerifyThen(testCase);
            }
            finally
            {
                NuLinkOutput = null;
            }
        }
        
        protected void ExecNuLinkIn(string directory, params string[] args)
        {
            string[] output;
            
            if (TestEnvironment.ShouldUseInstalledNuLinkBinary)
            {
                output = ExternalProgram.ExecIn(directory, TestEnvironment.InstalledNuLinkBinaryPath, args);
            }
            else
            {
                output = ExternalProgram.ExecIn(
                    directory, 
                    "dotnet",
                    new[] { TestEnvironment.CompiledNuLinkBinaryPath }.Concat(args).ToArray());
            }

            NuLinkOutput.AddRange(output);
        }
        
        private void Cleanup(AcceptanceTestCase testCase)
        {
            var target = testCase.Target;

            NuLinkOutput = new List<string>();
            
            ExternalProgram.ExecIn(TestEnvironment.DemoFolder, "git", "clean", "-dfx");
            ExternalProgram.ExecIn(TestEnvironment.DemoFolder, "git", "checkout", ".");

            var allSolitionFolders = testCase.Given.Packages
                .Select(p => target.PackageSolutionFolder(target.PackageId(p.Key)))
                .Append(target.ConsumerSolutionFolder)
                .ToArray();

            foreach (var solutionFodler in allSolitionFolders)
            {
                foreach (var package in testCase.Given.Packages)
                {
                    var packageId = target.PackageId(package.Key);
                    var packageFolder = target.PackageNugetFolder(
                        target.PackageProjectFolder(solutionFodler),
                        packageId);

                    if (Directory.Exists(packageFolder))
                    {
                        Directory.Delete(packageFolder, recursive: true);
                    }
                }
            }
            
            target.RestoreSolutionPackagesIn(target.ConsumerSolutionFolder);
        }
        
        private void SetupGiven(AcceptanceTestCase testCase)
        {
            var target = testCase.Target;
            
            foreach (var package in testCase.Given.Packages)
            {
                var packageId = target.PackageId(package.Key);
                SetupGivenPackageState(packageId, package.Value);
            }

            Directory.SetCurrentDirectory(testCase.GivenCurrentDiectory ?? TestEnvironment.DemoFolder);
            
            void SetupGivenPackageState(string packageId, PackageEntry package)
            {
                var packageSourceFolder = target.PackageSolutionFolder(packageId);
                var patchFilePath = Path.Combine(TestEnvironment.DemoFolder, "modify-test-case-packages.patch");

                if (package.State.HasFlag(PackageStates.Patched))
                {
                    ExternalProgram.ExecIn(packageSourceFolder, "git", "apply", "--ignore-whitespace", patchFilePath);
                }

                if (package.State.HasFlag(PackageStates.Built))
                {
                    target.BuildPackageProjectIn(packageSourceFolder);
                }

                if (package.State.HasFlag(PackageStates.Linked))
                {
                    ExecNuLinkIn(
                        testCase.Target.ConsumerSolutionFolder,
                        "link", 
                        "-p", packageId,
                        "-l", testCase.Target.PackageProjectFile(packageId));
                }
            }
        }

        private void VerifyThen(AcceptanceTestCase testCase)
        {
            var target = testCase.Target;
            
            VerifyPackages();
            VerifyNuLinkOutput();
            RunConsumerTests();
            
            void VerifyPackages()
            {
                foreach (var package in testCase.Then.Packages)
                {
                    var packageId = target.PackageId(package.Key);
                    VerifyThenPackageState(packageId, package.Value);
                }
            }

            void VerifyNuLinkOutput()
            {
                if (testCase.Then.ExpectedNuLinkOutput?.Count > 0)
                {
                    NuLinkOutput.Where(s => !string.IsNullOrEmpty(s)).ShouldBe(testCase.Then.ExpectedNuLinkOutput);
                }
            }

            void RunConsumerTests()
            {
                if (testCase.Then.ExpectedValues?.Count > 0)
                {
                    foreach (var expected in testCase.Then.ExpectedValues)
                    {
                        Environment.SetEnvironmentVariable($"TEST_{expected.Key}", expected.Value);
                    }

                    target.RunTestProjectIn(Path.Combine(
                        target.ConsumerSolutionFolder, 
                        "NuLink.TestCase.ConsumerLib.Tests"
                    ));
                }
            }

            void VerifyThenPackageState(string packageId, PackageEntry package)
            {
                var packageSolutionFolder = target.PackageSolutionFolder(packageId);
                var packageFolderPath = testCase.Target.PackageNugetFolder(packageSolutionFolder, packageId);
                var libFolderPath = testCase.Target.PackageNugetLibFolder(packageSolutionFolder, packageId, package.Version);
                var libFolderTargetPath = SymbolicLink.resolve(libFolderPath);
                var isLinked = (libFolderTargetPath != null && libFolderTargetPath != libFolderPath);
                var libBackupFolderExists = Directory.Exists(Path.Combine(packageFolderPath, package.Version, "nulink-backup.lib"));

                isLinked.ShouldBe(package.State.HasFlag(PackageStates.Linked));
                libBackupFolderExists.ShouldBe(package.State.HasFlag(PackageStates.Linked));
            }
        }
        
        public static IEnumerable<AcceptanceTestTarget> GetSupportedTargets()
        {
            yield return AcceptanceTestTarget.NetCore;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return AcceptanceTestTarget.NetFx;
            }
        }
    }
}
