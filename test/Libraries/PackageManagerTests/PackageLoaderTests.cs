using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Engine;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageLoaderTests : DynamoModelTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }
        public string PackagesDirectorySigned { get { return Path.Combine(TestDirectory, "pkgs_signed"); } }

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
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 16 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(16, loader.LocalPackages.Count());

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
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 16 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(16, loader.LocalPackages.Count());

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
        public void LoadingCustomNodeFromPackageSetsNodeInfoPackageInfoCorrectly()
        {
            var loader = new PackageLoader(PackagesDirectory);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

            // This test needs the "isTestMode" flag to be turned off as an exception to be able 
            // to test duplicate custom node def loading.
            loader.RequestLoadCustomNodeDirectory +=
                (dir, pkgInfo) => CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, isTestMode: false, packageInfo: pkgInfo);

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            var packageInfo = new PackageInfo("EvenOdd", new System.Version(1,0,0));
            var matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x => x.Value.PackageInfo.Equals(packageInfo)).ToList();
            //the node should have the correct package info and should be marked a packageMember.
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x=>x.Value.IsPackageMember == true));
        }

        [Test]
        public void PlacingCustomNodeInstanceFromPackageRetainsCorrectPackageInfoState()
        {
            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "EvenOdd");
            var package1 = Package.FromDirectory(packageDirectory, CurrentDynamoModel.Logger);
            loader.LoadPackages(new Package[] { package1});


            var packageInfo = new PackageInfo("EvenOdd", new System.Version(1, 0, 0));
            var matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x => x.Value.PackageInfo.Equals(packageInfo)).ToList();
            //the node should have the correct package info and should be marked a packageMember.
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x => x.Value.IsPackageMember == true));

            var cninst = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(matchingNodes.FirstOrDefault().Key, null, true);
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(cninst);

            matchingNodes = CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x => x.Value.PackageInfo.Equals(packageInfo)).ToList();
            Assert.AreEqual(1, matchingNodes.Count);
            Assert.IsTrue(matchingNodes.All(x => x.Value.IsPackageMember == true));
        }

 
        [Test]
        public void LoadingConflictingCustomNodePackageDoesNotGetLoaded()
        {
            var loader = new PackageLoader(PackagesDirectory);
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

            // This test needs the "isTestMode" flag to be turned off as an exception to be able 
            // to test duplicate custom node def loading.
            loader.RequestLoadCustomNodeDirectory +=
                (dir,pkgInfo) => CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir ,isTestMode: false, packageInfo: pkgInfo);

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = CurrentDynamoModel.PreferenceSettings,
                PathManager = CurrentDynamoModel.PathManager
            });

            // There are 16 packages in "Dynamo\test\pkgs"
            Assert.AreEqual(16, loader.LocalPackages.Count());

            var entries = CurrentDynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>();

            // Check that conflicting custom node package "EvenOdd2" is not installed
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd2\dyf")) == 0);
            Assert.IsTrue(entries.Count(x => Path.GetDirectoryName(x.Path).EndsWith(@"EvenOdd\dyf") && 
                                             x.FullName == "Test.EvenOdd") == 1);
        }

        [Test]
        public void LoadingConflictingCustomNodePackage_AfterPlacingNode_DoesNotGetLoaded()
        {
            var loader = GetPackageLoader();
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;


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
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

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
            loader.RequestLoadNodeLibrary += (libraryLoader as ExtensionLibraryLoader).LoadLibraryAndSuppressZTSearchImport;

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
                (dir,pkgInfo) => this.CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, true, pkgInfo);

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
            var loader = new PackageLoader(new [] {PackagesDirectory}, new [] {PackagesDirectorySigned});
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectorySigned, "Unsigned Package");
            var pkg = loader.ScanPackageDirectory(pkgDir, true);

            // Assert that ScanPackageDirectory returns no packages
            Assert.IsNull(pkg);
        }

        [Test]
        public void ScanPackageDirectoryWithCheckingCertificatesEnabledWillNotLoadPackageWithAlteredCertificate()
        {
            var loader = new PackageLoader(new[] { PackagesDirectory }, new[] { PackagesDirectorySigned });
            var libraryLoader = new ExtensionLibraryLoader(CurrentDynamoModel);

            loader.PackagesLoaded += libraryLoader.LoadPackages;
            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;

            var pkgDir = Path.Combine(PackagesDirectorySigned, "Modfied Signed Package");
            var pkg = loader.ScanPackageDirectory(pkgDir, true);

            // Assert that ScanPackageDirectory returns no packages
            Assert.IsNull(pkg);
        }

        [Test]
        public void ScanPackageDirectoryWithCheckingCertificatesEnabledWillLoadPackageWithValidCertificate()
        {
            var loader = new PackageLoader(new[] { PackagesDirectory }, new[] { PackagesDirectorySigned });
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
        }

        //FYI this is the code for the SingedPackage zero-touch node used in the previous test
        //The built and signed dll lives in the test\pkgs_signed\Signed Package\bin directory
        //This is for furture reference if we need to recreate the package in the future
        //namespace SignedPackage
        //{
        //    public class SignedPackage
        //    {
        //        public string Hello()
        //        {
        //            return nameof(Hello);
        //        }
        //    }
        //}

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
