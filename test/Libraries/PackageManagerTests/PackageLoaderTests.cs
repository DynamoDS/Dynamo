using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageLoaderTests : DynamoModelTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void ScanPackageDirectoryReturnsPackageForValidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "Custom Rounding");
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(pkgDir);

            Assert.IsNotNull(pkg);
            Assert.AreEqual("CAAD_RWTH", pkg.Group);
            Assert.AreEqual("Custom Rounding", pkg.Name);
            Assert.AreEqual("0.1.4", pkg.VersionName);
            Assert.AreEqual("This collection of nodes allows rounding, rounding up and rounding down to a specified precision.", pkg.Description);
            Assert.AreEqual("Round Up To Precision - Rounds a number *up* to a specified precision, Round Down To Precision - "
                + "Rounds a number *down* to a specified precision, Round To Precision - Rounds a number to a specified precision", pkg.Contents);
            Assert.AreEqual("0.5.2.10107", pkg.EngineVersion);

            loader.LoadPackages(new List<Package> {pkg});

            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);
        }

        [Test]
        public void PackageLoaderRequestsExtensionsBeLoaded()
        {
            var loader = GetPackageLoader();
            var pkgDir = Path.Combine(PackagesDirectory, "SampleExtension");

            var extensionLoad = false;
            var extensionAdd = false;
            var extensionReady = false;
            var packageLoaded = false;

            loader.PackgeLoaded += (package) =>
            {
                packageLoaded = true;
            };

            loader.RequestLoadExtension += (extensionPath) =>
            {
                extensionLoad = true;
                var mockExtension = new Moq.Mock<IExtension>();
                mockExtension.Setup(ext => ext.Startup(It.IsAny<StartupParams>())).Callback(() => { Assert.Fail(); });
                mockExtension.Setup(ext => ext.Ready(It.IsAny<ReadyParams>()))
               .Callback(() => { extensionReady = true; });
                return mockExtension.Object;
            };
            loader.RequestAddExtension += (extension) =>
            {
                extensionAdd = true;
            };

            var pkg = loader.ScanPackageDirectory(pkgDir);
            loader.LoadPackages(new List<Package> {pkg});

            Assert.IsTrue(loader.RequestedExtensions.Count() == 1);
            Assert.IsTrue(extensionLoad);
            Assert.IsTrue(extensionAdd);
            Assert.IsTrue(extensionReady);
            Assert.IsTrue(packageLoaded);
        }

        [Test]
        public void PackageLoaderDoesNotRequestsViewExtensionsBeLoaded()
        {
            var loader = GetPackageLoader();
            var pkgDir = Path.Combine(PackagesDirectory, "SampleViewExtension");

            var viewExtensionLoad = false;
            var viewExtensionAdd = false;

            loader.RequestLoadExtension += (extensionPath) =>
            {
                viewExtensionLoad = true;
                return null;
            };
            loader.RequestAddExtension += (extension) =>
            {
                viewExtensionAdd = true;
            };

            var pkg = loader.ScanPackageDirectory(pkgDir);
            loader.LoadPackages(new List<Package> {pkg});

            Assert.IsTrue(!loader.RequestedExtensions.Any());
            Assert.IsFalse(viewExtensionLoad);
            Assert.IsFalse(viewExtensionAdd);
        }

        [Test]
        public void ScanPackageDirectoryReturnsNullForInvalidDirectory()
        {
            var pkgDir = "";
            var loader = GetPackageLoader();
            Assert.IsNull(loader.ScanPackageDirectory(pkgDir));
        }

        [Test]
        public void LoadPackagesReturnsAllValidPackagesInValidDirectory()
        {
            var loader = new PackageLoader(PackagesDirectory);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 13 packages in "GitHub\Dynamo\test\pkgs"
            Assert.AreEqual(13, loader.LocalPackages.Count());

            // Verify that interdependent packages are resolved successfully
            var libs = CurrentDynamoModel.LibraryServices.ImportedLibraries.ToList();
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "AnotherPackage", "bin", "AnotherPackage.dll"))));
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "Dependent Package", "bin", "DependentPackage.dll"))));
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "Package", "bin", "Package.dll"))));

            // Verify that interdependent packages are imported successfully
            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Any(x => x.FullName == "AnotherPackage.AnotherPackage.AnotherPackage.HelloAnotherWorld"));
            Assert.IsTrue(entries.Any(x => x.FullName == "DependentPackage.DependentPackage.DependentPackage.HelloWorld"));
            Assert.IsTrue(entries.Any(x => x.FullName == "Package.Package.Package.Hello"));
        }

        [Test]
        public void LoadingPackageDoesNotAffectLoadedSearchEntries()
        {
            var loader = new PackageLoader(PackagesDirectory);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 13 packages in "GitHub\Dynamo\test\pkgs"
            Assert.AreEqual(13, loader.LocalPackages.Count());

            // Simulate loading new package from PM
            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\ZTTestPackage");
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> {pkg});

            // Assert that node belonging to new package is imported
            var node = GetNodeInstance("ZTTestPackage.RRTestClass.RRTestClass");
            Assert.IsNotNull(node);

            // Check that node belonging to one of the preloaded packages exists and is unique
            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Count(x => x.FullName == "AnotherPackage.AnotherPackage.AnotherPackage.HelloAnotherWorld") == 1);
        }

        [Test]
        public void LoadingConflictingCustomNodePackageDoesNotGetLoaded()
        {
            var loader = new PackageLoader(PackagesDirectory);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;
            loader.RequestLoadCustomNodeDirectory += 
                (dir) => CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, true);

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 13 packages in "GitHub\Dynamo\test\pkgs"
            Assert.AreEqual(13, loader.LocalPackages.Count());

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            // Check that conflicting custom node package "EvenOdd2" is not installed
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd2\dyf")) == 0);
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") && 
                                             x.FullName == "Test.EvenOdd") == 1);
        }

        [Test]
        public void LoadPackagesReturnsNoPackagesForInvalidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "No directory");
            var loader = new PackageLoader(pkgDir);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = this.CurrentDynamoModel.PreferenceSettings
            });

            Assert.AreEqual(0, loader.LocalPackages.Count());
        }

        [Test]
        public void GetOwnerPackageReturnsPackageForValidFunctionDefinition()
        {
            var loader = new PackageLoader(PackagesDirectory);
            loader.RequestLoadCustomNodeDirectory +=
                (dir) => this.CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, true);

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = this.CurrentDynamoModel.PreferenceSettings
            });

            var pkg = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);

            foreach (var nodeInfo in pkg.LoadedCustomNodes)
            {
                CustomNodeDefinition funcDef;
                Assert.IsTrue(this.CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, true, out funcDef));
                Assert.IsNotNull(funcDef);

                var foundPkg = loader.GetOwnerPackage(nodeInfo);

                Assert.IsNotNull(foundPkg);
                Assert.AreEqual(pkg.Name, foundPkg.Name);
                Assert.IsTrue(pkg.Name == foundPkg.Name);
            }
        }

        [Test]
        public void GetOwnerPackageReturnsNullForInvalidFunction()
        {
            var loader = new PackageLoader(PackagesDirectory);

            CustomNodeInfo info;
            Assert.IsTrue(
                this.CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(new string[] { TestDirectory, "core", "combine", "combine2.dyf" }),
                    true,
                    out info));

            CustomNodeDefinition funcDef;
            Assert.IsTrue(this.CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(info.FunctionId, true, out funcDef));
            var foundPkg = loader.GetOwnerPackage(info);
            Assert.IsNull(foundPkg);
        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidFunctionDefinition()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidType()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidPath()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void CanGetPackageFromRootReturnsPackageForValidDirectory()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void CanGetPackageFromRootReturnsNullForInvalidDirectory()
        {
            Assert.Inconclusive("Finish me");

        }

    }
}
