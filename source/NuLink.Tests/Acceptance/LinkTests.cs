using System;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class LinkTests : AcceptanceTestBase
    {
        [Test]
        public void DoNotPatch_DoNotLink_PatchesNotReflected()
        {
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Original)
                    }
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Original)
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
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.PatchedAndBuilt)
                    }
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Original)
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
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        ConsumerSolutionFolder,
                        "link",
                        "-p",
                        "NuLink.TestCase.SecondPackage",
                        "-l",
                        PackageProjectFile("NuLink.TestCase.SecondPackage"));
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Original),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Linked)
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
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        ConsumerSolutionFolder,
                        "link",
                        "-p",
                        "NuLink.TestCase.FirstPackage",
                        "-l",
                        PackageProjectFile("NuLink.TestCase.FirstPackage"));
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Linked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Original)
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
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.PatchedAndBuilt),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.PatchedAndBuilt)
                    }
                },
                When = () => {
                    ExecNuLinkIn(
                        ConsumerSolutionFolder, 
                        "link", 
                        "-p", "NuLink.TestCase.FirstPackage",
                        "-l", PackageProjectFile("NuLink.TestCase.FirstPackage"));
                    ExecNuLinkIn(
                        ConsumerSolutionFolder, 
                        "link", 
                        "-p", "NuLink.TestCase.SecondPackage",
                        "-l", PackageProjectFile("NuLink.TestCase.SecondPackage"));
                },
                Then = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Linked),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Linked)
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
