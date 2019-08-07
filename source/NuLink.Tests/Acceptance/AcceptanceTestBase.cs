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
        
        protected string[] Exec(string program, params string[] args)
        {
            return ExecIn(TestEnvironment.RepoFolder, program, args);
        }

        protected string[] ExecIn(string directory, string program, params string[] args)
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

            return output.ToArray();
        }

        protected void ExecNuLinkIn(string directory, params string[] args)
        {
            string[] output;
            
            if (TestEnvironment.ShouldUseInstalledNuLinkBinary)
            {
                output = ExecIn(directory, TestEnvironment.InstalledNuLinkBinaryPath, args);
            }
            else
            {
                output = ExecIn(
                    directory, 
                    "dotnet",
                    new[] { TestEnvironment.CompiledNuLinkBinaryPath }.Concat(args).ToArray());
            }

            NuLinkOutput.AddRange(output);
        }
        
        private void Cleanup(AcceptanceTestCase testCase)
        {
            NuLinkOutput = new List<string>();
            
            ExecIn(TestEnvironment.DemoFolder, "git", "clean", "-dfx");
            ExecIn(TestEnvironment.DemoFolder, "git", "checkout", ".");

            foreach (var package in testCase.Given.Packages)
            {
                var packageFolder = testCase.Target.PackageNugetFolder(package.Key);
                if (Directory.Exists(packageFolder))
                {
                    Directory.Delete(packageFolder, recursive: true);
                }
            }
            
            ExecIn(testCase.Target.ConsumerSolutionFolder, "dotnet", "restore");
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
                    ExecIn(packageSourceFolder, "git", "apply", "--ignore-whitespace", patchFilePath);
                }

                if (package.State.HasFlag(PackageStates.Built))
                {
                    ExecIn(packageSourceFolder, "dotnet", "build", "-c", "Debug");
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
            VerifyPackages();
            VerifyNuLinkOutput();
            RunConsumerTests();
            
            void VerifyPackages()
            {
                foreach (var package in testCase.Then.Packages)
                {
                    VerifyThenPackageState(package.Key, package.Value);
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

                    Exec("dotnet", "test", Path.Combine(
                        testCase.Target.ConsumerSolutionFolder, 
                        "NuLink.TestCase.ConsumerLib.Tests"
                    ));
                }
            }

            void VerifyThenPackageState(string packageId, PackageEntry package)
            {
                var packageFolderPath = testCase.Target.PackageNugetFolder(packageId);
                var libFolderPath = testCase.Target.PackageNugetLibFolder(packageId, package.Version);
                var libFolderTargetPath = SymbolicLink.resolve(libFolderPath);
                var isLinked = (libFolderTargetPath != null && libFolderTargetPath != libFolderPath);
                var libBackupFolderExists = Directory.Exists(Path.Combine(packageFolderPath, package.Version, "nulink-backup.lib"));

                isLinked.ShouldBe(package.State.HasFlag(PackageStates.Linked));
                libBackupFolderExists.ShouldBe(package.State.HasFlag(PackageStates.Linked));
            }
        }
    }
}
