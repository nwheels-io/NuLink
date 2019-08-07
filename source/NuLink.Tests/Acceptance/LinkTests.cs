using System;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class LinkTests : AcceptanceTestBase
    {
        [Test]
        public void DoNotPatch_DoNotLink_PatchesNotReflected()
        {
            var target = AcceptanceTestTarget.NetCore;
            
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

        [Test]
        public void PatchAll_DoNotLink_PatchesNotReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

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

        [Test]
        public void PatchAll_LinkLeaf_OnlyPatchOfLeafReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

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
                        "NuLink.TestCase.SecondPackage",
                        "-l",
                        target.PackageProjectFile("NuLink.TestCase.SecondPackage"));
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
        public void PatchAll_LinkNonLeaf_OnlyPatchOfNonLeafReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

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
                        "NuLink.TestCase.FirstPackage",
                        "-l",
                        target.PackageProjectFile("NuLink.TestCase.FirstPackage"));
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
        public void PatchAll_LinkAll_AllPatchesReflected()
        {
            var target = AcceptanceTestTarget.NetCore;

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
                        "-p", "NuLink.TestCase.FirstPackage",
                        "-l", target.PackageProjectFile("NuLink.TestCase.FirstPackage"));
                    ExecNuLinkIn(
                        target.ConsumerSolutionFolder, 
                        "link", 
                        "-p", "NuLink.TestCase.SecondPackage",
                        "-l", target.PackageProjectFile("NuLink.TestCase.SecondPackage"));
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
        public void PatchAndLinkAll_DotNetRestore_AllLinksSurvived()
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
                    ExecIn(
                        target.ConsumerSolutionFolder, 
                        "dotnet", 
                        "restore",
                        "--force");
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
