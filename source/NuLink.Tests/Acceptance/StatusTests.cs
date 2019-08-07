using System.IO;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class StatusTests : AcceptanceTestBase
    {
        [Test]
        public void NotLinked_PrintOK()
        {
            var target = AcceptanceTestTarget.NetCore;

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Original)
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
                        "NuLink.TestCase.FirstPackage 0.1.0-beta1 ok",
                        "NuLink.TestCase.SecondPackage 0.2.0-beta2 ok"
                    }
                }
            });
        }

        [Test]
        public void Linked_PrintLinkTargetPath()
        {
            var target = AcceptanceTestTarget.NetCore;
            var secondPackageTargetPath = Path.Combine(target.PackageProjectFolder("NuLink.TestCase.SecondPackage"), "bin", "Debug");
            
            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.PatchedBuiltAndLinked)
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
                        $"NuLink.TestCase.FirstPackage 0.1.0-beta1 ok",
                        $"NuLink.TestCase.SecondPackage 0.2.0-beta2 ok -> {secondPackageTargetPath}"
                    }
                }
            });
        }
    }
}
