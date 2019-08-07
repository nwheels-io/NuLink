using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class UnlinkTests : AcceptanceTestBase
    {
        [Test]
        public void AllPatchedAndLinked_DoNotUnLink_AllPatchesReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedBuiltAndLinked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Linked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Linked)
                    },
                    ExpectedValues = {
                        ["ClassOneShouldUseLocallyLinkedPackage"] = "FIRST-CLASS-SYMLINKED",
                        ["ClassTwoShouldUseLocallyLinkedPackage"] = "SECOND-CLASS-SYMLINKED(FIRST-CLASS-SYMLINKED)"
                    }
                }
            });
        }

        [Test]
        public void AllPatchedAndLinked_UnLinkLeaf_UnlinkedPatchesNotReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedBuiltAndLinked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "unlink",
                        "-p", "NuLink.TestCase.SecondPackage");
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Linked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Original)
                    },
                    ExpectedValues = {
                        ["ClassOneShouldUseLocallyLinkedPackage"] = "FIRST-CLASS-SYMLINKED",
                        ["ClassTwoShouldUseLocallyLinkedPackage"] = "SECOND-CLASS-ORIGINAL(FIRST-CLASS-SYMLINKED)"
                    }
                }
            });
        }

        [Test]
        public void AllPatchedAndLinked_UnLinkNonLeaf_UnlinkedPatchesNotReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedBuiltAndLinked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "unlink",
                        "-p", "NuLink.TestCase.FirstPackage");
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Linked)
                    },
                    ExpectedValues = {
                        ["ClassOneShouldUseLocallyLinkedPackage"] = "FIRST-CLASS-ORIGINAL",
                        ["ClassTwoShouldUseLocallyLinkedPackage"] = "SECOND-CLASS-SYMLINKED(FIRST-CLASS-ORIGINAL)"
                    }
                }
            });
        }

        [Test]
        public void AllPatchedAndLinked_UnLinkAll_PatchesNotReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedBuiltAndLinked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "unlink",
                        "-p", "NuLink.TestCase.FirstPackage");
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "unlink",
                        "-p", "NuLink.TestCase.SecondPackage");
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Original)
                    },
                    ExpectedValues = {
                        ["ClassOneShouldUseLocallyLinkedPackage"] = "FIRST-CLASS-ORIGINAL",
                        ["ClassTwoShouldUseLocallyLinkedPackage"] = "SECOND-CLASS-ORIGINAL(FIRST-CLASS-ORIGINAL)"
                    }
                }
            });
        }
    }
}