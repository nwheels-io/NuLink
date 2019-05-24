using System;
using NUnit.Framework;

namespace NuLink.Tests.Acceptance
{
    public class LinkTests : AcceptanceTestBase
    {
        [Test]
        public void OriginalPackages()
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
        public void PatchedPackageNotTakenWhenNotLinked()
        {
            ExecuteTestCase(new AcceptanceTestCase {
                Given = {
                    Packages = {
                        ["NuLink.TestCase.FirstPackage"] = new PackageEntry("0.1.0-beta1", PackageStates.Patched),
                        ["NuLink.TestCase.SecondPackage"] = new PackageEntry("0.2.0-beta2", PackageStates.Patched)
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
    }
}
