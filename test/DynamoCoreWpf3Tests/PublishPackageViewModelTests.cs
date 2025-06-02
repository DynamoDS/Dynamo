using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Moq;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PublishPackageViewModelTests : DynamoViewModelUnitTest
    {

        [Test]
        public void AddingDyfRaisesCanExecuteChangeOnDelegateCommand()
        {

            var vm = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);

            //find a customnode to add to the package
            string packagedirectory = Path.Combine(TestDirectory, "pkgs");
            var packages = Directory.EnumerateDirectories(packagedirectory);
            var first = Path.GetFullPath(packages.First());
            string dyfpath = Path.Combine(first, "dyf");
            var customnodes = Directory.GetFiles(dyfpath);
            var firstnode = customnodes.First();

            Console.WriteLine("add node at" + firstnode + "to package");

            var canExecuteChangedFired = 0;
            vm.SubmitCommand.CanExecuteChanged += ((o, e) => { canExecuteChangedFired++; });
            //now add a customnode to the package
            vm.AddFile(firstnode);

            //assert that canExecute changed was fired one time 
            Assert.AreEqual(canExecuteChangedFired, 1);

        }

        [Test]
        public void SetsErrorState()
        {
            //open a dyf file and modify it
            string packagedirectory = Path.Combine(TestDirectory, "pkgs");
            var packages = Directory.EnumerateDirectories(packagedirectory);
            var first = Path.GetFullPath(packages.FirstOrDefault(x => Path.GetFileName(x).Equals("Custom Rounding")));
            string dyfpath = Path.Combine(first, "dyf");
            var customnodes = Directory.GetFiles(dyfpath);
            var firstnode = customnodes.First();

            OpenModel(firstnode);

            //add a preset so that customnode has changes that are unsaved
            GetModel().CurrentWorkspace.AddPreset("a useless preset", "some thing that will modify the definition",
                new List<Guid>() { GetModel().CurrentWorkspace.Nodes.First().GUID });

            Assert.IsTrue(GetModel().CurrentWorkspace.HasUnsavedChanges);

            //now try to upload this file
            var vm = new PublishPackageViewModel(this.ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);
            //now add a customnode to the package
            vm.AddFile(firstnode);
            Console.WriteLine("add node at" + firstnode + "to package");

            vm.PublishLocallyCommand.Execute();
            //assert that we have not uploaded the file or indicated that we have
            Assert.AreNotEqual(vm.UploadState, PackageUploadHandle.State.Uploaded);
            Console.WriteLine(vm.ErrorString);

        }

        [Test]
        public void CanRemoveCustomNodesWithIdenticalNames()
        {
            var vm = new PublishPackageViewModel(this.ViewModel);

            // Arrange
            var customNode1 = new CustomNodeDefinition(Guid.NewGuid(), "Geometry.Curve", new List<NodeModel>());
            var customNode2 = new CustomNodeDefinition(Guid.NewGuid(), "Building.Curve", new List<NodeModel>());
            var customNode3 = new CustomNodeDefinition(Guid.NewGuid(), "Curve", new List<NodeModel>());

            vm.CustomNodeDefinitions.Add(customNode1);
            vm.CustomNodeDefinitions.Add(customNode2);
            vm.CustomNodeDefinitions.Add(customNode3);

            // Initial Assert
            Assert.AreEqual(3, vm.CustomNodeDefinitions.Count);

            var item1 = new PackageItemRootViewModel("Geometry.Curve.dyf", "C:\\test\\Geometry.Curve.dyf");
            var item2 = new PackageItemRootViewModel("Building.Curve.dyf", "C:\\test\\Building.Curve.dyf");
            var item3 = new PackageItemRootViewModel("Curve.dyf", "C:\\test\\Curve.dyf");
            var item4 = new PackageItemRootViewModel("Curve.dll", "C:\\dummy\\Curve.dll");

            // Assert
            vm.RemoveSingleItem(item1, DependencyType.CustomNodePreview);
            Assert.AreEqual(2, vm.CustomNodeDefinitions.Count);

            vm.RemoveSingleItem(item2, DependencyType.CustomNodePreview);
            Assert.AreEqual(1, vm.CustomNodeDefinitions.Count);

            vm.RemoveSingleItem(item3, DependencyType.CustomNodePreview);
            Assert.AreEqual(0, vm.CustomNodeDefinitions.Count);

            //assert that the method does not throw exception when the item is not found
            Assert.DoesNotThrow(() => { vm.RemoveSingleItem(item4, DependencyType.Assembly); });
        }

        [Test]
        public void CanPublishLateInitializedJsonCustomNode()
        {

            string nodePath = Path.Combine(TestDirectory, "core", "CustomNodes", "jsonCustomNode.dyf");

            //add this customNode to the package without opening it.
            var vm = new PublishPackageViewModel(this.ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);
            vm.AddFile(nodePath);

            //assert we don't raise any exceptions during getAllFiles
            //- this will check the customNode has no unsaved changes.

            Assert.AreEqual(1, vm.CustomNodeDefinitions.Count);
            Assert.DoesNotThrow(() => { vm.GetAllFiles(); });
            Assert.AreEqual(nodePath, vm.GetAllFiles().First());

        }


        [Test]
        public void NewPackageDoesNotThrow_NativeBinaryIsAddedAsAdditionalFile_NotBinary()
        {
            string packagesDirectory = Path.Combine(TestDirectory, "pkgs");

            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new List<string> { packagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings
            });

            PublishPackageViewModel vm = null;
            var package = loader.LocalPackages.FirstOrDefault(x => x.Name == "package with native assembly");
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package, false);
            });

            Assert.AreEqual(2, vm.AdditionalFiles.Count);
            Assert.AreEqual(0, vm.Assemblies.Count);

            Assert.AreEqual(PackageUploadHandle.State.Ready, vm.UploadState);
        }

        [Test]
        public void NewPackageVersionUpload_DoesNotThrowExceptionWhenDLLIsLoadedSeveralTimes()
        {
            string packagesDirectory = Path.Combine(TestDirectory, "pkgs");

            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new List<string> { packagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings
            });

            PublishPackageViewModel vm = null;
            var package = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package, true);
            });

            //while uploading a new version retain option is true, and we add the already loaded assembly to the additional files list now,
            //and the state of the upload remains Ready.
            Assert.AreEqual(PackageUploadHandle.State.Ready, vm.UploadState);
        }

        [Test]
        public void NewPackageVersionUpload_CanAddAndRemoveFiles()
        {
            string packagesDirectory = Path.Combine(TestDirectory, "pkgs");
            string addFilePath = Path.Combine(packagesDirectory, "testFile.txt");
            PackageItemRootViewModel pkgItem = new PackageItemRootViewModel(new FileInfo(addFilePath));

            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new List<string> { packagesDirectory });

            var loader = new PackageLoader(pathManager.Object);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings
            });

            PublishPackageViewModel vm = null;
            var package = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package, true);
            });

            //since retain is true, we will retain both the (renamed)assembly and the additional file.
            //the already loaded assembly is added to the additional files list as well
            vm.AddFile(addFilePath);
            Assert.AreEqual(3, vm.AdditionalFiles.Count);

            vm.RemoveItemCommand.Execute(pkgItem);
            Assert.AreEqual(2, vm.AdditionalFiles.Count);

            //arrange node libraries
            var assem = vm.Assemblies.FirstOrDefault().Assembly;
            var nodeLibraryNames = (IEnumerable<string>)new[] { assem.FullName };

            //act
            var pa = PublishPackageViewModel.GetPackageAssembly(nodeLibraryNames, assem);

            //assert
            Assert.NotNull(pa.Assembly);
            Assert.AreEqual(pa.Assembly.FullName, assem.FullName);
            Assert.IsTrue(pa.IsNodeLibrary);
        }

        [Test]
        [Category("Failure")]
        [Category("TechDebt")] //when a package is published - it does not load its customNodes. This may be intentional.
        public void PublishingACustomNodeSetsPackageInfoCorrectly_()
        {
            var cnworkspace = this.GetModel().CustomNodeManager.CreateCustomNode("nodeToBePublished", "somecategory", "publish this node") as CustomNodeWorkspaceModel;
            var inputNode = new Symbol();
            inputNode.InputSymbol = "input;";
            cnworkspace.AddAndRegisterNode(inputNode);

            var tempPath = Path.Combine(TempFolder, "nodeToBePublished.dyf");
            cnworkspace.Save(tempPath, false, this.GetModel().EngineController);

            Assert.IsNull(GetModel().CustomNodeManager.NodeInfos[cnworkspace.CustomNodeId].PackageInfo);
            Assert.IsFalse(GetModel().CustomNodeManager.NodeInfos[cnworkspace.CustomNodeId].IsPackageMember);

            //now lets publish this node as a local package.
            var newPkgVm = new PublishPackageViewModel(ViewModel) { CustomNodeDefinitions = new List<CustomNodeDefinition>() { cnworkspace.CustomNodeDefinition } };
            newPkgVm.Name = "PublishingACustomNodeSetsPackageInfoCorrectly";
            newPkgVm.MajorVersion = "0";
            newPkgVm.MinorVersion = "0";
            newPkgVm.BuildVersion = "1";
            newPkgVm.PublishLocallyCommand.Execute();

            Assert.IsTrue(GetModel().GetPackageManagerExtension().PackageLoader.LocalPackages.Any
                (x => x.Name == "PublishingACustomNodeSetsPackageInfoCorrectly" && x.LoadState.State == PackageLoadState.StateTypes.Loaded && x.LoadedCustomNodes.Count == 1));


            Assert.AreEqual(new PackageInfo("PublishingACustomNodeSetsPackageInfoCorrectly", new Version(0, 0, 1))
                , GetModel().CustomNodeManager.NodeInfos[cnworkspace.CustomNodeId].PackageInfo);
            Assert.IsFalse(GetModel().CustomNodeManager.NodeInfos[cnworkspace.CustomNodeId].IsPackageMember);

        }

        [Test]
        [Category("Failure")]
        [Category("TechDebt")]
        public void PublishingCustomNodeAsNewVersionWorks_SetsPackageInfoCorrectly()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void AssertIsSubPathOfDeep_IsSuccessful()
        {
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);

            //arrange
            Dictionary<string, bool> testDirs = new Dictionary<string, bool> {
                { @"C:\Package\bin\Dir1|C:\Package\bin\Dir1\Dir2", true },
                { @"C:\Package\bin\Dir1|C:\Package\bin\Dir1", false },
                { @"C:\Package\bin\Dir1\Dir2\Dir3\Dir4\Dir5\Dir6\Dir7\Dir8\Dir8\Dir9\Dir10|C:\Package\bin\Dir1", false },
                { @"C:\Package\bin\Dir1|C:\Package\bin\Dir1\Dir2\Dir3\Dir4\Dir5\Dir6\Dir7\Dir8\Dir8\Dir9\Dir10", true },
                { @"bin\Dir1|bin\Dir1\Dir2", true },
            };

            //assert
            foreach (var testDir in testDirs)
            {
                var paths = testDir.Key.Split('|');
                Assert.AreEqual(testDir.Value, newPkgVm.IsSubPathOfDeep(new PackageItemRootViewModel(paths[0]), new PackageItemRootViewModel(paths[1])));
            }
        }
        [Test]
        public void AssertAddChildRecursively_IsSuccessful()
        {
            //arrange
            List<string> testDirs = new List<string> {
                { @"C:\Package\bin\Dir1\Dir3" },
                { @"C:\Package\bin\Dir2\Dir3" },
                { @"C:\Package\bin\Dir3\Dir4" },
            };
            var root = new PackageItemRootViewModel(@"C:\Package");

            //assert
            foreach (var testDir in testDirs)
            {
                root.AddChildRecursively(new PackageItemRootViewModel(testDir));
            }

            var bin = root.ChildItems.First();
            var d1 = bin.ChildItems.ElementAt(0);
            var d2 = bin.ChildItems.ElementAt(1);
            var d3 = bin.ChildItems.ElementAt(2);


            Assert.IsTrue(root.ChildItems.Count == 1);
            Assert.IsTrue(root.ChildItems.Select(x => x.DirectoryName.EndsWith("bin")).Any());

            Assert.IsTrue(bin.ChildItems.Count == 3);
            Assert.IsTrue(bin.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir1")).Any());
            Assert.IsTrue(bin.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir2")).Any());
            Assert.IsTrue(bin.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir3")).Any());

            Assert.IsTrue(d1.ChildItems.Count == 1);
            Assert.IsTrue(d2.ChildItems.Count == 1);
            Assert.IsTrue(d3.ChildItems.Count == 1);
            Assert.IsTrue(d1.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir3")).Any());
            Assert.IsTrue(d2.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir3")).Any());
            Assert.IsTrue(d3.ChildItems.Select(x => x.DirectoryName.EndsWith("Dir4")).Any());

            Assert.IsTrue(d1.ChildItems.First().ChildItems.Count == 0);
            Assert.IsTrue(d2.ChildItems.First().ChildItems.Count == 0);
            Assert.IsTrue(d3.ChildItems.First().ChildItems.Count == 0);
        }

        [Test]
        public void EnsureMultipleVersionsOfAssembly_CannotBeLoaded()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);

            //arrange the first assembly version
            string packagedirectory = Path.Combine(TestDirectory, "pkgs\\PackageManager\\1.0");
            var version_1 = Directory.GetFiles(packagedirectory);
            var firstAssembly = version_1.First();

            //add the first assembly version file
            vm.AddFile(firstAssembly);

            //assert that we have successfully added the assembly
            Assert.AreEqual(vm.Assemblies.Count, 1);

            //arrange the second assembly version
            packagedirectory = Path.Combine(TestDirectory, "pkgs\\PackageManager\\2.0");
            var version_2 = Directory.GetFiles(packagedirectory);
            var secondAssembly = version_2.First();

            //now add the second assembly version file
            vm.AddFile(secondAssembly);

            //TODO: assert - do we expect to see 1 or 2 here?
            Assert.AreEqual(vm.Assemblies.Count, 1);
        }


        #region CreateContentsRelationships

        [Test]
        public void CreatesCorrectRelationships_ControlTest()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var items = new Dictionary<string, PackageItemRootViewModel>();
            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };

            foreach (var file in files)
            {
                var item = new PackageItemRootViewModel(file);
                if (String.IsNullOrEmpty(item.DirectoryName)) continue;
                if (!items.ContainsKey(item.DirectoryName))
                {
                    var root = new PackageItemRootViewModel(item.DirectoryName);

                    root.ChildItems.Add(item);
                    items[item.DirectoryName] = root;
                }
                else
                {
                    items[item.DirectoryName].ChildItems.Add(item);
                }
            }

            List<PackageItemRootViewModel> result = vm.BindParentToChild(items);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().ChildItems.Count);
        }


        [Test]
        public void CreatesCorrectRelationships_UncommonRootsTest()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var items = new Dictionary<string, PackageItemRootViewModel>();
            var files = new[] { new FileInfo(@"D:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };

            foreach (var file in files)
            {
                var item = new PackageItemRootViewModel(file);
                if (String.IsNullOrEmpty(item.DirectoryName)) continue;
                if (!items.ContainsKey(item.DirectoryName))
                {
                    var root = new PackageItemRootViewModel(item.DirectoryName);

                    root.ChildItems.Add(item);
                    items[item.DirectoryName] = root;
                }
                else
                {
                    items[item.DirectoryName].ChildItems.Add(item);
                }
            }

            var result = vm.BindParentToChild(items);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result.First().ChildItems.Count);
        }

        [Test]
        public void FindCommonPaths_SingleRoot()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Users\\Alice\\Documents\\Report.docx",
                "C:\\Users\\Alice\\Documents\\Resume.pdf",
                "C:\\Users\\Alice\\Documents\\Presentation.pptx"
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(1, commonPaths.Count);
            Assert.AreEqual("C:\\Users\\Alice\\Documents", commonPaths.First());

        }

        [Test]
        public void FindCommonPaths_MultipleRootsSingleFiles()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Users\\Alice\\Documents\\Report.docx",
                "D:\\Users\\Alice\\Documents\\Presentation.pptx"
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(2, commonPaths.Count);
            Assert.AreEqual("C:\\Users\\Alice\\Documents", commonPaths[0]);
            Assert.AreEqual("D:\\Users\\Alice\\Documents", commonPaths[1]);

        }


        [Test]
        public void FindCommonPaths_MultipleRootsMultipleFiles()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Users\\Alice\\Documents\\Report.docx",
                "C:\\Users\\Alice\\Documents\\Subfolder\\Resume.pdf",
                "D:\\Users\\Alice\\Documents\\Presentation.pptx"
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(2, commonPaths.Count);
            Assert.AreEqual("C:\\Users\\Alice\\Documents", commonPaths[0]);
            Assert.AreEqual("D:\\Users\\Alice\\Documents", commonPaths[1]);
        }



        [Test]
        public void FindCommonPaths_MultipleRoots()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Users\\Alice\\Documents\\Report.docx",
                "D:\\Users\\Alice\\Documents\\Resume.pdf",
                "E:\\Users\\Alice\\Documents\\Presentation.pptx"
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(3, commonPaths.Count);
            Assert.AreEqual("C:\\Users\\Alice\\Documents", commonPaths[0]);
            Assert.AreEqual("D:\\Users\\Alice\\Documents", commonPaths[1]);
            Assert.AreEqual("E:\\Users\\Alice\\Documents", commonPaths[2]);
        }


        [Test]
        public void FindCommonPaths_MultipleRootsSingleCommonPath()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Packages\\PackageTest\\Loc1\\Sub11\\test.docx",
                "C:\\Packages\\PackageTest\\Loc2\\Sub21\\test2.docx",
                "C:\\Packages\\PackageTest2\\Loc1\\test.dyn",
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(1, commonPaths.Count);
            Assert.AreEqual("C:\\Packages", commonPaths[0]);
        }


        [Test]
        public void FindCommonPaths_BaseRoot()
        {
            var vm = new PublishPackageViewModel(ViewModel);
            var paths = new string[]
            {
                "C:\\Report.docx",
                "C:\\Users\\Alice\\Documents\\Subfolder\\Resume.pdf",
            };

            var commonPaths = vm.GetCommonPaths(paths);

            Assert.AreEqual(1, commonPaths.Count);
            Assert.AreEqual("C:\\", commonPaths[0]);
        }

        #endregion

    }
}
