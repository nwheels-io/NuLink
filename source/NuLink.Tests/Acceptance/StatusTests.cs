using System.IO;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class StatusTests : AcceptanceTestBase
    {
        [TestCaseSource(nameof(GetSupportedTargets))]
        public void NotLinked_PrintOK(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Original)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        Path.Combine(target.ConsumerSolutionFolder, "NuLink.TestCase.ConsumerLib"),
                        "status",
                        "-q");
                },
                Then = {
                    ExpectedNuLinkOutput = new[] {
                        "NuLink.TestCase.FirstPackage 0.1.0 ok",
                        "NuLink.TestCase.SecondPackage 0.2.0 ok"
                    }
                }
            });
        }

        [TestCaseSource(nameof(GetSupportedTargets))]
        public void Linked_PrintLinkTargetPath(AcceptanceTestTarget target)
        {
            target.LogName();

            var secondPackageTargetPath = Path.Combine(target.PackageProjectFolder("NuLink.TestCase.SecondPackage"), "bin", "Debug");
            
            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        Path.Combine(target.ConsumerSolutionFolder, "NuLink.TestCase.ConsumerLib"),
                        "status",
                        "-q");
                },
                Then = {
                    ExpectedNuLinkOutput = new[] {
                        $"NuLink.TestCase.FirstPackage 0.1.0 ok",
                        $"NuLink.TestCase.SecondPackage 0.2.0 ok -> {secondPackageTargetPath}"
                    }
                }
            });
        }
    }
}
