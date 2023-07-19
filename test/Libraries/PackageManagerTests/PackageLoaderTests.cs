using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;
using Dynamo.Interfaces;

namespace Dynamo.PackageManager.Tests
{
    class PackageLoaderTests : DynamoModelTestBase
    {
        private const string builtinPackRootDirName = @"Built-In Packages";

        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }
        public string PackagesDirectorySigned { get { return Path.Combine(TestDirectory, "pkgs_signed"); } }
        internal string BuiltInPackagesTestDir { get { return Path.Combine(TestDirectory, "builtinpackages testdir", "Packages"); } }

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
        public void LoadPackagesDoesNotDuplicateLoadedAssemblies()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "AnotherPackage");
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(pkgDir);

            loader.LoadPackages(new List<Package> { pkg });
            Assert.AreEqual(1, pkg.LoadedAssemblies.Count);
            Assert.AreEqual("AnotherPackage", pkg.LoadedAssemblies.First().Name);
            Assert.IsTrue(pkg.LoadedAssemblies.First().IsNodeLibrary);
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

            Action<Package> pkgLoadedDelegate = (package) =>
            {
                packageLoaded = true;
            };
            loader.PackgeLoaded += pkgLoadedDelegate;

            Func<string, IExtension> reqLoadExtnDelegate = (extensionPath) =>
            {
                extensionLoad = true;
                var mockExtension = new Moq.Mock<IExtension>();
                mockExtension.Setup(ext => ext.Startup(It.IsAny<StartupParams>())).Callback(() => { Assert.Fail(); });
                mockExtension.Setup(ext => ext.Ready(It.IsAny<ReadyParams>()))
               .Callback(() => { extensionReady = true; });
                return mockExtension.Object;
            };
            loader.RequestLoadExtension += reqLoadExtnDelegate;

            Action<IExtension> reqAddExtnDelegate = (extension) =>
            {
                extensionAdd = true;
            };
            loader.RequestAddExtension += reqAddExtnDelegate;

            var pkg = loader.ScanPackageDirectory(pkgDir);
            loader.LoadPackages(new List<Package> {pkg});

            Assert.IsTrue(loader.RequestedExtensions.Count() == 1);
            Assert.IsTrue(extensionLoad);
            Assert.IsTrue(extensionAdd);
            Assert.IsTrue(extensionReady);
            Assert.IsTrue(packageLoaded);

            loader.PackgeLoaded -= pkgLoadedDelegate;
            loader.RequestLoadExtension -= reqLoadExtnDelegate;
            loader.RequestAddExtension -= reqAddExtnDelegate;
        }

        [Test]
        public void PackageDoesNotReloadOnAbsenceOfNewPackagePath()
        {
            var pathManager = new PathManager(new PathManagerParams { });

            var settings = new PreferenceSettings();
            settings.CustomPackageFolders = new List<string> { PackagesDirectory };
            pathManager.Preferences = settings;

            var loader = new PackageLoader(pathManager);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packagesLoaded = false;

            Action<IEnumerable<Assembly>> pkgsLoadedDelegate = (x) => { packagesLoaded = true; };
            loader.PackagesLoaded += pkgsLoadedDelegate;

            var loadPackageParams = new LoadPackageParams
            {
                Preferences = settings,
                
            };
            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(19, loader.LocalPackages.Count());
            Assert.AreEqual(true, packagesLoaded);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Count(x => x.FullName == "Package.Package.Package.Hello") == 1);

            packagesLoaded = false;
            // This function is called upon addition of new package paths in the UI.
            loader.LoadNewCustomNodesAndPackages(new List<string>(), CurrentDynamoModel.CustomNodeManager);
            Assert.AreEqual(19, loader.LocalPackages.Count());

            // Assert packages are not reloaded if there are no new package paths.
            Assert.False(packagesLoaded);

            // Assert there are no duplication of nodes after trying to reload packages.
            Assert.IsTrue(entries.Count(x => x.FullName == "Package.Package.Package.Hello") == 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
            loader.PackagesLoaded -= pkgsLoadedDelegate;
        }

        [Test]
        public void NoPackageNodeDuplicatesOnAddingNewPackagePath()
        {
            var pathManager = new PathManager(new PathManagerParams { });

            var settings = new PreferenceSettings();
            settings.CustomPackageFolders = new List<string> { PackagesDirectory };
            pathManager.Preferences = settings;

            var loader = new PackageLoader(pathManager);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packagesLoaded = false;
            Action<IEnumerable<Assembly>> pkgsLoadedDelegate = (x) => { packagesLoaded = true; };
            loader.PackagesLoaded += pkgsLoadedDelegate;

            var loadPackageParams = new LoadPackageParams
            {
                Preferences =settings,
            };
            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(19, loader.LocalPackages.Count());
            Assert.AreEqual(true, packagesLoaded);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Count(x => x.FullName == "Package.Package.Package.Hello") == 1);

            packagesLoaded = false;
            settings.CustomPackageFolders = new List<string> { PackagesDirectory, BuiltInPackagesTestDir };
            var newPaths = new List<string> { Path.Combine(TestDirectory, "builtinpackages testdir") };
            // This function is called upon addition of new package paths in the UI.
            loader.LoadNewCustomNodesAndPackages(newPaths, CurrentDynamoModel.CustomNodeManager);
            Assert.AreEqual(20, loader.LocalPackages.Count());

            // Assert packages are reloaded if there are new package paths.
            Assert.True(packagesLoaded);

            // Assert there are no duplication of nodes after trying to reload packages.
            Assert.IsTrue(entries.Count(x => x.FullName == "Package.Package.Package.Hello") == 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
            loader.PackagesLoaded -= pkgsLoadedDelegate;
        }

        [Test]
        public void BuiltInPackageUnloadedWhenDuplicateAlreadyLoaded()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;

            var pathManager = new PathManager(new PathManagerParams { });

            var settings = new PreferenceSettings();
            settings.CustomPackageFolders = new List<string> { PackagesDirectorySigned };
            pathManager.Preferences = settings;

            var loader = new PackageLoader(pathManager);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var loadPackageParams = new LoadPackageParams
            {
                Preferences = settings
            };
            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(3, loader.LocalPackages.Count());
            Assert.IsTrue(loader.LocalPackages.Count(x => x.Name == "SignedPackage") == 1);

            settings.CustomPackageFolders = new List<string> { PackagesDirectory, BuiltInPackagesTestDir };
            var newPaths = new List<string> { Path.Combine(TestDirectory, "builtinpackages testdir") };
            // This function is called upon addition of new package paths in the UI.
            loader.LoadNewCustomNodesAndPackages(newPaths, CurrentDynamoModel.CustomNodeManager);
            Assert.AreEqual(4, loader.LocalPackages.Count());

            Assert.IsTrue(loader.LocalPackages.Count(x => x.Name == "SignedPackage") == 2);

            var nonBuitlIn = loader.LocalPackages.FirstOrDefault(x => !x.BuiltInPackage);
            Assert.IsNotNull(nonBuitlIn);
            Assert.AreEqual(nonBuitlIn.LoadState.State, PackageLoadState.StateTypes.Loaded);

            var buitlIn = loader.LocalPackages.FirstOrDefault(x => x.BuiltInPackage);
            Assert.IsNotNull(buitlIn);
            Assert.AreEqual(buitlIn.LoadState.State, PackageLoadState.StateTypes.Unloaded);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.AreEqual(entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"), 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void DisableBuiltInPackagePath_DoesNotLoadBuiltInPackage()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;

            var pathManager = new Mock<IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { BuiltInPackagesTestDir });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>();
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            };

            // Disable built-in package location.
            (loadPackageParams.Preferences as IDisablePackageLoadingPreferences).DisableBuiltinPackages = true;
            loader.LoadAll(loadPackageParams);

            Assert.AreEqual(0, loader.LocalPackages.Count());

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void DisableCustomPackagePath_DoesNotLoadCustomPackages()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;

            var pathManager = new Mock<IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectorySigned, BuiltInPackagesTestDir });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string> { PackagesDirectorySigned };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            };

            // Disable custom package location.
            (loadPackageParams.Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations = true;
            loader.LoadAll(loadPackageParams);

            // Only built-in package is expected to be loaded.
            Assert.AreEqual(1, loader.LocalPackages.Count());
            var buitlIn = loader.LocalPackages.FirstOrDefault(x => x.BuiltInPackage);
            Assert.IsNotNull(buitlIn);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.AreEqual(entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"), 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void DisableThenEnableCustomPackagePath_LoadsCustomPackages()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;

            var pathManager = new Mock<IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectorySigned, BuiltInPackagesTestDir });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string> { PackagesDirectorySigned };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            };

            // Disable custom package location.
            (loadPackageParams.Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations = true;
            loader.LoadAll(loadPackageParams);

            // Only built-in package is expected to be loaded.
            Assert.AreEqual(1, loader.LocalPackages.Count());
            var buitlIn = loader.LocalPackages.FirstOrDefault(x => x.BuiltInPackage);
            Assert.IsNotNull(buitlIn);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.AreEqual(entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"), 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            // Reset package loader for new round of loading packages.
            loader = new PackageLoader(pathManager.Object);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            // Re-enable custom package location.
            (loadPackageParams.Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations = false;
            loader.LoadAll(loadPackageParams);

            Assert.AreEqual(4, loader.LocalPackages.Count());

            entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.AreEqual(entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"), 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void DisableBuiltInPackagePath_LoadsCustomPackages()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;

            var pathManager = new Mock<IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectorySigned, BuiltInPackagesTestDir });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string> { PackagesDirectorySigned };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            };

            // Disable built-in package location.
            (loadPackageParams.Preferences as IDisablePackageLoadingPreferences).DisableBuiltinPackages = true;
            loader.LoadAll(loadPackageParams);

            Assert.AreEqual(3, loader.LocalPackages.Count());
            var buitlIn = loader.LocalPackages.FirstOrDefault(x => x.BuiltInPackage);
            Assert.IsNull(buitlIn);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void PackageLoaderDoesNotRequestsViewExtensionsBeLoaded()
        {
            var loader = GetPackageLoader();
            var pkgDir = Path.Combine(PackagesDirectory, "SampleViewExtension");

            var viewExtensionLoad = false;
            var viewExtensionAdd = false;

            Func<string, IExtension> reqLoadExtnDelegate = (extensionPath) =>
            {
                viewExtensionLoad = true;
                return null;
            };
            loader.RequestLoadExtension += reqLoadExtnDelegate;

            Action<IExtension> reqAddExtnDelegate = (extension) =>
            {
                viewExtensionAdd = true;
            };
            loader.RequestAddExtension += reqAddExtnDelegate;

            var pkg = loader.ScanPackageDirectory(pkgDir);
            loader.LoadPackages(new List<Package> {pkg});

            Assert.IsTrue(!loader.RequestedExtensions.Any());
            Assert.IsFalse(viewExtensionLoad);
            Assert.IsFalse(viewExtensionAdd);

            loader.RequestLoadExtension -= reqLoadExtnDelegate;
            loader.RequestAddExtension -= reqAddExtnDelegate;
        }

        [Test]
        public void ScanPackageDirectoryReturnsNullForInvalidDirectory()
        {
            var pkgDir = "";
            var loader = GetPackageLoader();
            Assert.IsNull(loader.ScanPackageDirectory(pkgDir));
        }

        [Test]
        public void LoadingBuiltInZTPackageAddsItToLibrary()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new List<string> { BuiltInPackagesTestDir });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packagesLoaded = false;
            loader.PackagesLoaded += (x) => { packagesLoaded = true; };

            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>();
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,

            };
            loader.LoadAll(loadPackageParams);

            Assert.AreEqual(1, loader.LocalPackages.Count());
            Assert.AreEqual(true, packagesLoaded);

            Assert.IsTrue(CurrentDynamoModel.SearchModel.SearchEntries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello") == 1);
        }

        [Test]
        public void LoadPackagesReturnsAllValidPackagesInValidDirectory()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            });

            // There are 19 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(19, loader.LocalPackages.Count());

            // Verify that interdependent packages are resolved successfully
            // TODO: Review these assertions. Lambdas are not using x, so they are basically just checking that test files exist.
            var libs = CurrentDynamoModel.LibraryServices.ImportedLibraries.ToList();
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "AnotherPackage", "bin", "AnotherPackage.dll"))));
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "Dependent Package", "bin", "DependentPackage.dll"))));
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "Package", "bin", "Package.dll"))));

            // Verify that interdependent packages are imported successfully
            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Any(x => x.FullName == "AnotherPackage.AnotherPackage.AnotherPackage.HelloAnotherWorld"));
            Assert.IsTrue(entries.Any(x => x.FullName == "DependentPackage.DependentPackage.DependentPackage.HelloWorld"));
            Assert.IsTrue(entries.Any(x => x.FullName == "Package.Package.Package.Hello"));

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void LoadingPackageDoesNotAffectLoadedSearchEntries()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            });

            // There are 19 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(19, loader.LocalPackages.Count());

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

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void LoadingCustomNodeFromPackageSetsNodeInfoPackageInfoCorrectly()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            // This test needs the "isTestMode" flag to be turned off as an exception to be able 
            // to test duplicate custom node def loading.
            Func<string, PackageInfo, IEnumerable<CustomNodeInfo>> reqLoadCNDelegate = (dir, pkgInfo) => {
                Console.WriteLine($"packageInfo reqLoadCNDelegate :{pkgInfo}");
                return CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, isTestMode: false, packageInfo: pkgInfo);
                };
            loader.RequestLoadCustomNodeDirectory += reqLoadCNDelegate;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            });

            var packageInfo = new PackageInfo("EvenOdd", new System.Version(1,0,0));

            //this test fails randomly - log some info to help debug it.
            Console.WriteLine($"pathmanager.PackagesDirectories{String.Join(",",pathManager.Object.PackagesDirectories)}");
            var matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x =>
            {
                Console.WriteLine($"val {x.Value}, name {x.Value.Name}, pkginfo {x.Value.PackageInfo}, packagemember {x.Value.IsPackageMember}");
                return x.Value.PackageInfo.Equals(packageInfo);
            }
            ).ToList();
            //the node should have the correct package info and should be marked a packageMember.
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x=>x.Value.IsPackageMember == true));

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
            loader.RequestLoadCustomNodeDirectory -= reqLoadCNDelegate;
        }

        [Test]
        public void PlacingCustomNodeInstanceFromPackageRetainsCorrectPackageInfoState()
        {
            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "EvenOdd");
            var package1 = Package.FromDirectory(packageDirectory, CurrentDynamoModel.Logger);
            loader.LoadPackages(new Package[] { package1});


            var packageInfo = new PackageInfo("EvenOdd", new System.Version(1, 0, 0));
            //this test fails randomly - log some info to help debug it.
            Console.WriteLine($"pathmanager.PackagesDirectories{String.Join(",", CurrentDynamoModel.PathManager.PackagesDirectories)}");
            var matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x =>
            {
                Console.WriteLine($"val{x.Value}, name{x.Value.Name}, pkginfo{x.Value.PackageInfo}, packagemember{x.Value.IsPackageMember}");
                return x.Value.PackageInfo.Equals(packageInfo);
            }
            ).ToList();
            //the node should have the correct package info and should be marked a packageMember.
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x => x.Value.IsPackageMember == true));

            var cninst = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(matchingNodes.FirstOrDefault().Key, null, true);
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(cninst);

            matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x => x.Value.PackageInfo.Equals(packageInfo)).ToList();
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x => x.Value.IsPackageMember == true));

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

 
        [Test]
        public void LoadingConflictingCustomNodePackageDoesNotGetLoaded()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            // This test needs the "isTestMode" flag to be turned off as an exception to be able 
            // to test duplicate custom node def loading.
            Func<string, PackageInfo, IEnumerable<CustomNodeInfo>> reqLoadCNDelegate = (dir, pkgInfo) => 
            CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, isTestMode: false, packageInfo: pkgInfo);
            loader.RequestLoadCustomNodeDirectory += reqLoadCNDelegate;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
            });

            // There are 19 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(19, loader.LocalPackages.Count());

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            // Check that conflicting custom node package "EvenOdd2" is not installed
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd2\dyf")) == 0);
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") && 
                                             x.FullName == "Test.EvenOdd") == 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
            loader.RequestLoadCustomNodeDirectory -= reqLoadCNDelegate;
        }

        [Test]
        public void LoadingConflictingCustomNodePackage_AfterPlacingNode_DoesNotGetLoaded()
        {
            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;


            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "EvenOdd");
            var packageDirectory2 = Path.Combine(TestDirectory, "pkgs", "EvenOdd2");
            var package1 = Package.FromDirectory(packageDirectory,CurrentDynamoModel.Logger);
            var package2 = Package.FromDirectory(packageDirectory2, CurrentDynamoModel.Logger);
            loader.LoadPackages(new Package[] { package1,package2 });

            // 2 packages loaded as expected
            var expectedLoadedPackageNum = 0;
            foreach (var pkg in loader.LocalPackages)
            {
                if (pkg.Name == "EvenOdd" || pkg.Name == "EvenOdd2")
                {
                    expectedLoadedPackageNum++;
                }
            }
            Assert.AreEqual(2, expectedLoadedPackageNum);

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            // Check that conflicting custom node package "EvenOdd2" is not installed
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd2\dyf")) == 0);
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") &&
                                             x.FullName == "Test.EvenOdd") == 1);

            var customNodeInstance = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"));
            //this will reset the info.
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(customNodeInstance);

            //load again.
            loader.LoadPackages(new Package[] { package1, package2 });

            //reassert conflicting package not loaded.
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd2\dyf")) == 0);
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") &&
                                             x.FullName == "Test.EvenOdd") == 1);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        // This can occur when a user copies a custom node from a package into definitions folder.
        // TODO not exactly clear what behavior should be.
        [Test]
        [Category("TechDebt")]
        [Category("Failure")]
        public void CreatingConflictingCustomNodeWithPackage_WillOverwriteCustomNodeInPackage_GUIDStillOwnedByPackage()
        {
            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "EvenOdd");
            var package1 = Package.FromDirectory(packageDirectory, CurrentDynamoModel.Logger);
            loader.LoadPackages(new Package[] { package1 });

            // There is 1 package loaded directly
            Assert.AreEqual(1, loader.LocalPackages.Count());

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") &&
                                             x.FullName == "Test.EvenOdd") == 1);

            var customNodeWS = CurrentDynamoModel.CustomNodeManager.CreateCustomNode("aConlictingNode",
                "aCategory",
                "a node that will conflict via id with loaded package",
                Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"));
            customNodeWS.AddAndRegisterNode(new Output());
            Assert.AreEqual(1, customNodeWS.Nodes.Count());
            var tempPath = GetNewFileNameOnTempPath(".dyf");
            customNodeWS.Save(tempPath);

            var customNodeInstance = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"));
            //this will reset the info.
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(customNodeInstance);
            //the definition now points to the updated function
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>().FirstOrDefault().Definition.FunctionBody.Count());
            Assert.AreEqual(1, this.CurrentDynamoModel.CustomNodeManager.LoadedDefinitions.Where(x => x.FunctionId == customNodeInstance.Definition.FunctionId).FirstOrDefault().FunctionBody.Count());

            var matchingNode = CurrentDynamoModel.CustomNodeManager.NodeInfos[customNodeInstance.Definition.FunctionId];
            Assert.IsNotNull(matchingNode);
            //This still points to the package the guid came from - should it?
            Assert.False(matchingNode.IsPackageMember);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;

        }

        //TODO I cannot get this to replicate the behavior I see in Dynamo which that loading a package on top of a 
        //loose conflicting custom node will overwrite it and log an error message. (package overwrite message)
        [Test]
        [Category("TechDebt")]
        [Category("Failure")]
        public void LoadingAPackageThatConflictsWithLooseLoadedCustomNodeWillOverwriteLocalCustomNode()
        {
            var customNodeWS = CurrentDynamoModel.CustomNodeManager.CreateCustomNode("aConlictingNode",
             "aCategory",
             "a node that will conflict via id with loaded package",
             Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"));
            customNodeWS.AddAndRegisterNode(new Output());
            Assert.AreEqual(1, customNodeWS.Nodes.Count());
            //var tempPath = GetNewFileNameOnTempPath(".dyf");
            //customNodeWS.Save(tempPath);

            //var customNodeInstance = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"));
            //this will reset the info.
            //this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(customNodeInstance);
          

            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "EvenOdd");
            var package1 = Package.FromDirectory(packageDirectory, CurrentDynamoModel.Logger);
            loader.LoadPackages(new Package[] { package1 });

            // There is 1 package loaded directly
            Assert.AreEqual(1, loader.LocalPackages.Count());

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") &&
                                             x.FullName == "Test.EvenOdd") == 1);

            Assert.IsTrue(entries.Count(x =>x.FullName == "aCategory.aConlictingNode") == 0);

            var customNodeInstance2 = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(Guid.Parse("3f19c484-d3f3-49ba-88c2-6c386a41f6ac"), "Test.EvenOdd",true);
            //this will reset the info.
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(customNodeInstance2);
            Assert.AreEqual(3, customNodeInstance2.InPorts.Count());
            //the definition now points to the updated function
            //Assert.AreEqual(4, this.CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>().LastOrDefault().Definition.FunctionBody.Count());
            //Assert.AreEqual(4, this.CurrentDynamoModel.CustomNodeManager.LoadedDefinitions.Where(x => x.FunctionId == customNodeInstance2.Definition.FunctionId).Count());
            

            var matchingNode = CurrentDynamoModel.CustomNodeManager.NodeInfos[customNodeInstance2.Definition.FunctionId];
            Assert.IsNotNull(matchingNode);
            //This still points to the package the guid came from - should it?
            Assert.True(matchingNode.IsPackageMember);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void LoadPackagesReturnsNoPackagesForInvalidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "No directory");
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { pkgDir });

            var loader = new PackageLoader(pathManager.Object);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings
            });

            Assert.AreEqual(0, loader.LocalPackages.Count());
        }

        [Test]
        public void GetOwnerPackageReturnsPackageForValidFunctionDefinition()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);

            Func<string, PackageInfo, IEnumerable<CustomNodeInfo>> reqLoadCNDelegate = 
                (dir, pkgInfo) => CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, true, pkgInfo);
            loader.RequestLoadCustomNodeDirectory += reqLoadCNDelegate;
                
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings
            });

            var pkg = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);

            foreach (var nodeInfo in pkg.LoadedCustomNodes)
            {
                CustomNodeDefinition funcDef;
                Assert.IsTrue(CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, true, out funcDef));
                Assert.IsNotNull(funcDef);

                var foundPkg = loader.GetOwnerPackage(nodeInfo);

                Assert.IsNotNull(foundPkg);
                Assert.AreEqual(pkg.Name, foundPkg.Name);
                Assert.IsTrue(pkg.Name == foundPkg.Name);
            }

            loader.RequestLoadCustomNodeDirectory -= reqLoadCNDelegate;
        }

        [Test]
        public void GetOwnerPackageReturnsNullForInvalidFunction()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new List<string> { PackagesDirectory });

            var loader = new PackageLoader(pathManager.Object);

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(new string[] { TestDirectory, "core", "combine", "combine2.dyf" }),
                    true,
                    out info));

            CustomNodeDefinition funcDef;
            Assert.IsTrue(this.CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(info.FunctionId, true, out funcDef));
            var foundPkg = loader.GetOwnerPackage(info);
            Assert.IsNull(foundPkg);
        }

        [Test]
        public void PackageLoadExceptionTest()
        {
            string openPath = Path.Combine(TestDirectory, @"core\PackageLoadExceptionTest.dyn");
            OpenModel(openPath);

            var loader = GetPackageLoader();

            // Load the package when the graph is open in the workspace. 
            string packageDirectory = Path.Combine(PackagesDirectory, "Ampersand");
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> { pkg });

            // Dummy nodes are resolved, and more importantly, no exception was thrown.
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
        }

        /// <summary>
        /// This test simulates loading a mixed package type (containing both CN and ZT nodes)
        /// on the fly and verifying that the nodes in the graph are resolved and run afterwards.
        /// </summary>
        [Test]
        public void MixedPackageLoadTest()
        {
            string openPath = Path.Combine(TestDirectory, @"core\MixedPackageLoadTest.dyn");
            OpenModel(openPath);

            AssertPreviewValue("654bfcc3463e4950824336d4c9bd6126", null);
            AssertPreviewValue("576f11ed5837460d80f2e354d853de68", null);

            var loader = GetPackageLoader();

            // Load the package when the graph is open in the workspace. 
            string packageDirectory = Path.Combine(PackagesDirectory, "Mixed Package");
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> { pkg });

            var libs = CurrentDynamoModel.LibraryServices.ImportedLibraries.ToList();
            Assert.IsTrue(libs.Any(x => File.Exists(Path.Combine(PackagesDirectory, "Mixed Package", "bin", "FrogRiverOne.dll"))));

            // Assert value of loaded ZT node.
            AssertPreviewValue("654bfcc3463e4950824336d4c9bd6126", 9);

            // Assert value of loaded CN is non-null.
            AssertNonNull("576f11ed5837460d80f2e354d853de68");
        }

        [Test]
        public void LoadingAPackageWithBinariesDoesNotAffectCustomNodesUsedInHomeWorkspace()
        {
            // Open a custom node definition and a workspace where this custom node is used.
            OpenModel(Path.Combine(TestDirectory, @"core\PackageLoadReset\test.dyf"));
            OpenModel(Path.Combine(TestDirectory, @"core\PackageLoadReset\MissingNode.dyn"));

            // Get the custom node.
            var functionNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>();
            Assert.AreEqual(1, functionNodes.Count());
            var functionNode = functionNodes.First();

            // Custom node should be good before loading the package.
            Assert.AreEqual(ElementState.Active, functionNode.State);
            AssertPreviewValue(functionNode.AstIdentifierGuid, 7);

            // Load a package which contains binaries, when the graph is open in the workspace. 
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(Path.Combine(PackagesDirectory, "Mixed Package"));
            loader.LoadPackages(new List<Package> { pkg });

            // Custom node should remain good after loading the package.
            Assert.AreEqual(ElementState.Active, functionNode.State);
            AssertPreviewValue(functionNode.AstIdentifierGuid, 7);
        }

        [Test]
        public void ScanPackageDirectoryWithCheckingCertificatesEnabledWillNotLoadPackageWithoutValidCertificate()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => PackagesDirectorySigned );

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectorySigned, "Unsigned Package");
            var pkg = loader.ScanPackageDirectory(pkgDir, true);

            // Assert that ScanPackageDirectory returns no packages
            Assert.IsNull(pkg);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
        }

        [Test]
        public void ScanPackageDirectoryWithCheckingCertificatesEnabledWillNotLoadPackageWithAlteredCertificate()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => PackagesDirectorySigned);

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectorySigned, "Modfied Signed Package");
            var pkg = loader.ScanPackageDirectory(pkgDir, true);

            // Assert that ScanPackageDirectory returns no packages
            Assert.IsNull(pkg);

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
        }
        [Test]
        public void ScanPackageDirectoryWithCheckingCertificatesEnabledWillLoadPackageWithValidCertificate()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => PackagesDirectorySigned);

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectorySigned, "Signed Package");
            var pkg = loader.ScanPackageDirectory(pkgDir, true);

            // Assert that ScanPackageDirectory returns a package
            Assert.IsNotNull(pkg);
            Assert.IsTrue(pkg.RequiresSignedEntryPoints);
            loader.LoadPackages(new List<Package> { pkg });

            // Verify that package resolved successfully
            var libs = CurrentDynamoModel.LibraryServices.ImportedLibraries.ToList();
            Assert.IsTrue(libs.Contains(Path.Combine(PackagesDirectorySigned, "Signed Package", "bin", "SignedPackage.dll")));

            // Verify that the package are imported successfully
            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            Assert.IsTrue(entries.Any(x => x.FullName == "SignedPackage.SignedPackage.SignedPackage.Hello"));

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
        }

        //signedpackage generated from internal repo at SignedDynamoTestingPackages

        [Test]
        public void HasValidBuiltinPackagesAndDefaultPackagesPath()
        {
            // Arrange
            var pathManager = CurrentDynamoModel.PathManager as PathManager;
            var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(pathManager.GetType()).Location),
               builtinPackRootDirName, PathManager.PackagesDirectoryName);

            // Act
            var builtinpackageLocation = PathManager.BuiltinPackagesDirectory;
            var defaultDirectory = pathManager.DefaultPackagesDirectory;

            // Assert
            Assert.That(builtinpackageLocation,Is.Not.Null.Or.Empty);
            Assert.IsTrue(string.Equals(builtinpackageLocation, directory, StringComparison.OrdinalIgnoreCase));
            Assert.AreNotEqual(defaultDirectory, directory);
        }

        [Test]
        public void PackageInBuiltinPackageLocationIsLoaded()
        {
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;
            //setup clean loader
            var loader = new PackageLoader(CurrentDynamoModel.PathManager);
            var settings = new PreferenceSettings();
            settings.DisableBuiltinPackages = false;

            var loaderParams = new LoadPackageParams()
            {
                Preferences = settings
            };
            //invoke the load
            loader.LoadAll(loaderParams);

            //assert the package in builtIn packages was loaded.
            Assert.IsTrue(loader.LocalPackages.Any(x => x.BinaryDirectory.Contains("SignedPackage2")));
            Assert.AreEqual(1, loader.LocalPackages.Count());
            
        }

        [Test]
        public void DisablingBuiltinPackagesCorrectlyDisablesLoading()
        {

            //setup clean loader
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;
            var loader = new PackageLoader(CurrentDynamoModel.PathManager);
            var settings = new PreferenceSettings();
            settings.DisableBuiltinPackages = true;

            var loaderParams = new LoadPackageParams()
            { 
                Preferences = settings 
            };
            //then invoke load

            loader.LoadAll(loaderParams);

            //assert the package in builtIn packages was not loaded.
            Assert.IsFalse(loader.LocalPackages.Any(x => x.Name.Contains("SignedPackage2")));
            Assert.AreEqual(0, loader.LocalPackages.Count());
            
        }
        //signedpackge2 generated from internal repo at SignedDynamoTestingPackages

        [Test]
        public void PackageInCustomPackagePathIsLoaded()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(
                () => new List<string> { BuiltInPackagesTestDir });
            
            //setup clean loader where builtIn packages is a custom package path
            var loader = new PackageLoader(pathManager.Object);
            var settings = new PreferenceSettings();
            //just to be certain this is false.
            settings.DisableCustomPackageLocations = false;
            settings.CustomPackageFolders = new List<string>() { BuiltInPackagesTestDir };
            var loaderParams = new LoadPackageParams()
            {
                Preferences = settings
            };
            //invoke the load
            loader.LoadAll(loaderParams);

            //assert the package in the custom package path was loaded
            Assert.IsTrue(loader.LocalPackages.Any(x => x.BinaryDirectory.Contains("SignedPackage2")));
            Assert.AreEqual(1, loader.LocalPackages.Count());
            
        }
        [Test]
        public void DisablingCustomPackagePathsCorrectlyDisablesLoading()
        {
            //setup clean loader where builtIn packages is a custom package path
            PathManager.BuiltinPackagesDirectory = BuiltInPackagesTestDir;
            var loader = new PackageLoader(CurrentDynamoModel.PathManager);
            var settings = new PreferenceSettings();
            //disable custom package paths
            settings.DisableCustomPackageLocations = true;
            settings.CustomPackageFolders = new List<string>() { BuiltInPackagesTestDir };
            var loaderParams = new LoadPackageParams()
            {
                Preferences = settings
            };
            //invoke the load
            loader.LoadAll(loaderParams);

            //assert the package in the custom package path was not loaded
            Assert.IsFalse(loader.LocalPackages.Any(x => x.BuiltInPackage));
            Assert.AreEqual(0, loader.LocalPackages.Count());
           
        }

        [Test]
        public void PackageLoaderLoadPackageWithBadVersion()
        {
            // Arrange
            var loader = GetPackageLoader();
            var badPackageLocation = Path.Combine(PackagesDirectory, @"BadVersion\PackageWithBadVersion");

            // Act
            var badPackage = loader.ScanPackageDirectory(badPackageLocation);

            // Assert
            Assert.IsNull(badPackage);
            Assert.IsNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Bad package"));
        }

        [Test]
        public void PackageLoaderLoadMultiplePackagesWithBadVersion()
        {
            // Arrange
            var loader = GetPackageLoader();
            var goodPackageLocation = Path.Combine(PackagesDirectory, @"BadVersion\PackageWithGoodVersion");
            var badPackageLocation = Path.Combine(PackagesDirectory, @"BadVersion\PackageWithBadVersion");

            // Act
            var goodPackage = loader.ScanPackageDirectory(goodPackageLocation);
            var badPackage = loader.ScanPackageDirectory(badPackageLocation);

            // Assert
            Assert.IsNotNull(goodPackage);
            Assert.IsNull(badPackage);
            Assert.IsNotNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Good package"));
            Assert.IsNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Bad package"));
        }

        [Test]
        public void PackageLoaderLoadMultiplePackagesWithBadVersionReversed()
        {
            // Arrange
            var loader = GetPackageLoader();
            var goodPackageLocation = Path.Combine(PackagesDirectory, @"BadVersion\PackageWithGoodVersion");
            var badPackageLocation = Path.Combine(PackagesDirectory, @"BadVersion\PackageWithBadVersion");

            // Act
            var badPackage = loader.ScanPackageDirectory(badPackageLocation);
            var goodPackage = loader.ScanPackageDirectory(goodPackageLocation);

            // Assert
            Assert.IsNotNull(goodPackage);
            Assert.IsNull(badPackage);
            Assert.IsNotNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Good package"));
            Assert.IsNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Bad package"));
        }


        [Test]
        public void PackageLoaderLoadNewPackage()
        {
            // Arrange
            var loader = GetPackageLoader();
            var oldPackageLocation = Path.Combine(PackagesDirectory, @"Version\PackageWithOldVersion");
            var newPackageLocation = Path.Combine(PackagesDirectory, @"Version\PackageWithNewVersion");

            // Act
            var oldPackage = loader.ScanPackageDirectory(oldPackageLocation);
            var newPackage = loader.ScanPackageDirectory(newPackageLocation);

            // Assert
            Assert.IsNotNull(oldPackage);
            Assert.IsNull(newPackage);
            Assert.AreEqual("Package", oldPackage.Name);
            Assert.AreEqual("1.0.0", oldPackage.VersionName);
            Assert.IsNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"New package"));
            Assert.IsNotNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Old package"));
        }

        [Test]
        public void PackageLoaderLoadOldPackage()
        {
            // Arrange
            var loader = GetPackageLoader();
            var oldPackageLocation = Path.Combine(PackagesDirectory, @"Version\PackageWithOldVersion");
            var newPackageLocation = Path.Combine(PackagesDirectory, @"Version\PackageWithNewVersion");

            // Act
            var newPackage = loader.ScanPackageDirectory(newPackageLocation);
            var oldPackage = loader.ScanPackageDirectory(oldPackageLocation);

            // Assert
            Assert.IsNull(oldPackage);
            Assert.IsNotNull(newPackage);
            Assert.AreEqual("Package", newPackage.Name);
            Assert.AreEqual("2.0.0", newPackage.VersionName);
            Assert.IsNotNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"New package"));
            Assert.IsNull(loader.LocalPackages.FirstOrDefault(package => package.Description == @"Old package"));
        }

        //TODO this test should probably be removed after refactor that Aparajit is doing.
        [Test]
        public void BuiltInPackagesIsNotExposedInPathManager()
        {
            // Arrange
            var pathManager = CurrentDynamoModel.PathManager;

            // Act
            var defaultPackageDirectory = pathManager.DefaultPackagesDirectory;
            var packageDirectories = pathManager.PackagesDirectories;
            var defaultUserDefinitions = pathManager.DefaultUserDefinitions;
            var userDefinitions = pathManager.DefinitionDirectories;

            // Assert
            const string ExpectedToken = @"%BuiltInPackages%";
            Assert.AreNotEqual(ExpectedToken, defaultPackageDirectory);
            Assert.AreEqual(3, packageDirectories.Count());
            Assert.IsFalse(packageDirectories.Contains(ExpectedToken));
            Assert.IsTrue(packageDirectories.Contains(PathManager.BuiltinPackagesDirectory));
            Assert.AreNotEqual(ExpectedToken, defaultUserDefinitions);
            Assert.AreEqual(3, userDefinitions.Count());
            Assert.IsFalse(userDefinitions.Contains(ExpectedToken));
            Assert.IsTrue(userDefinitions.Contains(PathManager.BuiltinPackagesDirectory));
        }

        [Test]
        public void PathManagerDefaultPackagesDirectory()
        {
            var pathManager = CurrentDynamoModel.PathManager;
            var settings = CurrentDynamoModel.PreferenceSettings;

            Assert.NotNull(settings);
            Assert.NotNull(pathManager);

            // The default selected package path for install in preference settings is AppData
            // if not set from the UI.
            var selectedPackagePathInstallDir = settings.SelectedPackagePathForInstall;
            var appDataFolder = GetAppDataFolder();
            Assert.AreEqual(appDataFolder, selectedPackagePathInstallDir);

            var fullPath = Path.Combine(appDataFolder, PathManager.PackagesDirectoryName);
            Assert.AreEqual(fullPath, pathManager.DefaultPackagesDirectory);

            // The preference setting SelectedPackagePathForInstall property affects
            // the DefaultPackagesDirectory property of the PathManager as this is how the 
            // the package download path is actually set in the package manager client.
            settings.SelectedPackagePathForInstall = Path.GetTempPath();
            Assert.AreEqual(Path.GetTempPath(), pathManager.DefaultPackagesDirectory);
        }

        [Test]
        [Category("FailureNET6")]
        public void LocalizedPackageLocalizedCorrectly()
        {
            var esculture = CultureInfo.CreateSpecificCulture("es-ES");

            // Save current culture - usually "en-US"
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            // Set "es-ES"
            Thread.CurrentThread.CurrentCulture = esculture;
            Thread.CurrentThread.CurrentUICulture = esculture;

            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => string.Empty);

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectory, "Dynamo Samples");
            var pkg = loader.ScanPackageDirectory(pkgDir, false);

            // Assert that ScanPackageDirectory returns a package
            Assert.IsNotNull(pkg);
            loader.LoadPackages(new List<Package> { pkg });       

            // Verify that the package are imported successfully
            var entries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();
            var nse = entries.Where(x => x.FullName == "SampleLibraryUI.Examples.LocalizedNode").FirstOrDefault();
            Assert.IsNotNull(nse);

            //verify that the node has the correctly localized description
            Assert.AreEqual("Un nodo de interfaz de usuario de muestra que muestra una interfaz de usuario personalizada."
                , nse.Description);
            var node = nse.CreateNode();
            Assert.AreEqual("Un nodo de interfaz de usuario de muestra que muestra una interfaz de usuario personalizada."
               , nse.Description);

            // Restore "en-US"
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
        }

        [Test]
        public void PackageContainingNodeViewOnlyCustomization_AddsCorrectAssembliesToNodeModelLoader()
        {
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => string.Empty);

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,

            };
            loader.LoadAll(loadPackageParams);
            //verify the UI assembly was imported from the correct package.
            var testPackage = loader.LocalPackages.FirstOrDefault(x => x.Name == "NodeViewCustomizationTestPackage");
            var uiassembly = testPackage.LoadedAssemblies.FirstOrDefault(a => a.Name == "NodeViewCustomizationAssembly");
            Assert.IsNotNull(uiassembly);
            //verify is marked as nodelib
            Assert.IsTrue(uiassembly.IsNodeLibrary);
            //verify that no zero touch method in that assembly was imported.
            var importedFunctionGroups = CurrentDynamoModel.LibraryServices.ImportedFunctionGroups.ToList();
            var searchEntries = CurrentDynamoModel.SearchModel.SearchEntries.ToList();

            Assert.IsTrue(importedFunctionGroups.Count(x => x.QualifiedName.Contains("NodeModelAssembly.TestClass.TestFunc")) == 0);
            Assert.IsTrue(importedFunctionGroups.Count(x => x.QualifiedName.Contains("NodeViewCustomizationAssembly.TestClass2.TestFunc2")) == 0);
            //verify that the nodemodel is imported!
            Assert.IsTrue(searchEntries.Count(x => x.FullName.Contains("NodeModelAssembly.NodeModelDerivedClass")) == 1);

            //verify customization assembly was added to nodeModelAssemblyLoader
            Assert.IsTrue(CurrentDynamoModel.Loader.LoadedAssemblies.Select(x=>x.FullName.Contains("NodeViewCustomizationAssembly")).Any());
            Assert.IsTrue(CurrentDynamoModel.Loader.LoadedAssemblies.Select(x => x.FullName.Contains("NodeModelAssembly")).Any());

            loader.PackagesLoaded -= libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
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
