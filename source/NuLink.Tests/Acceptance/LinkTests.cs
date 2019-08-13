using System;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class LinkTests : AcceptanceTestBase
    {
        [TestCaseSource(nameof(GetSupportedTargets))]
        public void DoNotPatch_DoNotLink_PatchesNotReflected(AcceptanceTestTarget target)
        {
            target.LogName();
            
            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.Original)
                    }
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

        [TestCaseSource(nameof(GetSupportedTargets))]
        public void PatchAll_DoNotLink_PatchesNotReflected(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedAndBuilt)
                    }
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

        [TestCaseSource(nameof(GetSupportedTargets))]
        public void PatchAll_LinkLeaf_OnlyPatchOfLeafReflected(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "link",
                        "-p",
                        target.PackageId("NuLink.TestCase.SecondPackage"),
                        "-l",
                        target.PackageProjectFile(target.PackageId("NuLink.TestCase.SecondPackage")));
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

        [TestCaseSource(nameof(GetSupportedTargets))]
        public void PatchAll_LinkNonLeaf_OnlyPatchOfNonLeafReflected(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder,
                        "link",
                        "-p",
                        target.PackageId("NuLink.TestCase.FirstPackage"),
                        "-l",
                        target.PackageProjectFile(target.PackageId("NuLink.TestCase.FirstPackage")));
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
        
        [TestCaseSource(nameof(GetSupportedTargets))]
        public void PatchAll_LinkAll_AllPatchesReflected(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder, 
                        "link", 
                        "-p", target.PackageId("NuLink.TestCase.FirstPackage"),
                        "-l", target.PackageProjectFile(target.PackageId("NuLink.TestCase.FirstPackage")));
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder, 
                        "link", 
                        "-p", target.PackageId("NuLink.TestCase.SecondPackage"),
                        "-l", target.PackageProjectFile(target.PackageId("NuLink.TestCase.SecondPackage")));
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

        [TestCaseSource(nameof(GetSupportedTargets))]
        public void PatchAndLinkAll_DotNetRestore_AllLinksSurvived(AcceptanceTestTarget target)
        {
            target.LogName();

            ExecuteTestCase(new AcceptanceTestCase(target) {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0", PackageStates.PatchedBuiltAndLinked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0", PackageStates.PatchedBuiltAndLinked)
                    }
                },
                When = () => {
                    target.RestoreSolutionPackagesIn(target.ConsumerSolutionFolder);
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
    }
}
