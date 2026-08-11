using System.IO;

using Dynamo.Core;
using Dynamo.Utilities;

using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// DYN-10745 band-aid tests. Remove this file along with LegacyAssistantExtensionGuard
    /// once DYN-10739 lands the permanent fix.
    /// </summary>
    [TestFixture]
    class LegacyAssistantExtensionGuardTests
    {
        private string originalBuiltinPackagesDirectory;

        [SetUp]
        public void Setup()
        {
            originalBuiltinPackagesDirectory = PathManager.BuiltinPackagesDirectory;
            LegacyAssistantExtensionGuard.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            PathManager.BuiltinPackagesDirectory = originalBuiltinPackagesDirectory;
            LegacyAssistantExtensionGuard.Reset();
        }

        [Test]
        [Category("UnitTests")]
        [TestCase("AutodeskAssistant", "Autodesk Assistant")]
        [TestCase("DynamoAssistant", "Autodesk Assistant")]
        [TestCase("DynamoMCP", "DynamoMCP")]
        public void WhenPackageNameIsRestrictedThenDisplayNameIsReturned(string packageName, string expectedDisplayName)
        {
            Assert.IsTrue(LegacyAssistantExtensionGuard.TryGetRestrictedPackageDisplayName(packageName, out var displayName));
            Assert.AreEqual(expectedDisplayName, displayName);
        }

        [Test]
        [Category("UnitTests")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("SomeOtherPackage")]
        [TestCase("TuneUp")]
        public void WhenPackageNameIsNotRestrictedThenNoDisplayNameIsReturned(string packageName)
        {
            Assert.IsFalse(LegacyAssistantExtensionGuard.TryGetRestrictedPackageDisplayName(packageName, out var displayName));
            Assert.IsNull(displayName);
        }

        [Test]
        [Category("UnitTests")]
        [TestCase(LegacyAssistantExtensionGuard.AutodeskAssistantTypeName, "Autodesk Assistant")]
        [TestCase(LegacyAssistantExtensionGuard.McpViewExtensionTypeName, "DynamoMCP")]
        public void WhenViewExtensionTypeIsRestrictedThenDisplayNameIsReturned(string typeName, string expectedDisplayName)
        {
            Assert.IsTrue(LegacyAssistantExtensionGuard.TryGetRestrictedViewExtensionDisplayName(typeName, out var displayName));
            Assert.AreEqual(expectedDisplayName, displayName);
        }

        [Test]
        [Category("UnitTests")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Some.Other.ViewExtension")]
        public void WhenViewExtensionTypeIsNotRestrictedThenNoDisplayNameIsReturned(string typeName)
        {
            Assert.IsFalse(LegacyAssistantExtensionGuard.TryGetRestrictedViewExtensionDisplayName(typeName, out var displayName));
            Assert.IsNull(displayName);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPathIsUnderBuiltinPackagesDirectoryThenIsNotOutside()
        {
            PathManager.BuiltinPackagesDirectory = @"C:\Dynamo\Built-In Packages\Packages";
            var path = Path.Combine(PathManager.BuiltinPackagesDirectory, "AutodeskAssistant");

            Assert.IsFalse(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(path));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPathIsOutsideBuiltinPackagesDirectoryThenIsOutside()
        {
            PathManager.BuiltinPackagesDirectory = @"C:\Dynamo\Built-In Packages\Packages";

            Assert.IsTrue(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(@"C:\Users\someone\AppData\Roaming\Dynamo\Dynamo Core\4.2\packages\DynamoMCP"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPathIsUnderSimilarlyNamedSiblingDirectoryThenIsOutside()
        {
            // A plain StartsWith would let "Built-In PackagesOld" pass as if it were under
            // "Built-In Packages" -- the check must require a directory-separator boundary.
            PathManager.BuiltinPackagesDirectory = @"C:\Dynamo\Built-In Packages";

            Assert.IsTrue(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(@"C:\Dynamo\Built-In PackagesOld\AutodeskAssistant"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPathEqualsBuiltinPackagesDirectoryExactlyThenIsNotOutside()
        {
            PathManager.BuiltinPackagesDirectory = @"C:\Dynamo\Built-In Packages";

            Assert.IsFalse(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(@"C:\Dynamo\Built-In Packages"));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPathContainsDotDotSegmentsEscapingBuiltinDirectoryThenIsOutside()
        {
            PathManager.BuiltinPackagesDirectory = @"C:\Dynamo\Built-In Packages";

            Assert.IsTrue(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(
                @"C:\Dynamo\Built-In Packages\Packages\AutodeskAssistant\..\..\..\Escaped"));
        }

        [Test]
        [Category("UnitTests")]
        [TestCase(null)]
        [TestCase("")]
        public void WhenPathIsNullOrEmptyThenIsOutside(string path)
        {
            Assert.IsTrue(LegacyAssistantExtensionGuard.IsOutsideBuiltInPackages(path));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPackageBlockedThenItAppearsInAllBlockedPathsOnly()
        {
            LegacyAssistantExtensionGuard.RecordBlockedPackage(@"C:\packages\DynamoMCP");

            CollectionAssert.AreEquivalent(new[] { @"C:\packages\DynamoMCP" }, LegacyAssistantExtensionGuard.AllBlockedPaths);
            // Package blocks already raise their own notification via LibraryLoadFailedException,
            // so they must not also appear in the view-extension-only collection.
            Assert.AreEqual(0, LegacyAssistantExtensionGuard.BlockedViewExtensions.Count);
            Assert.IsTrue(LegacyAssistantExtensionGuard.HasBlockedPaths);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenViewExtensionBlockedThenBothFilesAppearInAllBlockedPaths()
        {
            LegacyAssistantExtensionGuard.RecordBlockedViewExtension(
                "Autodesk Assistant",
                @"C:\Revit\AddIns\DynamoForRevit\viewExtensions\AutodeskAssistant_ViewExtensionDefinition.xml",
                @"C:\Revit\AddIns\DynamoForRevit\AutodeskAssistantViewExtension.dll");

            Assert.AreEqual(1, LegacyAssistantExtensionGuard.BlockedViewExtensions.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    @"C:\Revit\AddIns\DynamoForRevit\viewExtensions\AutodeskAssistant_ViewExtensionDefinition.xml",
                    @"C:\Revit\AddIns\DynamoForRevit\AutodeskAssistantViewExtension.dll"
                },
                LegacyAssistantExtensionGuard.AllBlockedPaths);
            Assert.IsTrue(LegacyAssistantExtensionGuard.HasBlockedPaths);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNothingBlockedThenHasBlockedPathsIsFalse()
        {
            Assert.IsFalse(LegacyAssistantExtensionGuard.HasBlockedPaths);
            Assert.AreEqual(0, LegacyAssistantExtensionGuard.AllBlockedPaths.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenResetThenAllRecordedStateIsCleared()
        {
            LegacyAssistantExtensionGuard.RecordBlockedPackage(@"C:\packages\DynamoMCP");
            LegacyAssistantExtensionGuard.RecordBlockedViewExtension("Autodesk Assistant", @"C:\a.xml", @"C:\a.dll");

            LegacyAssistantExtensionGuard.Reset();

            Assert.IsFalse(LegacyAssistantExtensionGuard.HasBlockedPaths);
            Assert.AreEqual(0, LegacyAssistantExtensionGuard.BlockedViewExtensions.Count);
        }
    }
}
