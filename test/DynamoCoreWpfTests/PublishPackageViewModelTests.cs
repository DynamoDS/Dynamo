using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo;
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
    public class PublishPackageViewModelTests: DynamoViewModelUnitTest
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
                new List<Guid>(){GetModel().CurrentWorkspace.Nodes.First().GUID});

            Assert.IsTrue(GetModel().CurrentWorkspace.HasUnsavedChanges);

            //now try to upload this file
            var vm = new PublishPackageViewModel(this.ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);
            //now add a customnode to the package
            vm.AddFile(firstnode);
            Console.WriteLine("add node at" + firstnode + "to package");

            vm.PublishLocallyCommand.Execute();
            //assert that we have not uploaded the file or indicated that we have
            Assert.AreNotEqual(vm.UploadState,PackageUploadHandle.State.Uploaded);
            Console.WriteLine(vm.ErrorString);

        }

        [Test]
        public void CanPublishLateInitializedJsonCustomNode()
        {

            string nodePath = Path.Combine(TestDirectory,"core","CustomNodes", "jsonCustomNode.dyf");

            //add this customNode to the package without opening it.
            var vm = new PublishPackageViewModel(this.ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);
            vm.AddFile(nodePath);

            //assert we don't raise any exceptions during getAllFiles
            //- this will check the customNode has no unsaved changes.

            Assert.AreEqual(1, vm.CustomNodeDefinitions.Count);
            Assert.DoesNotThrow(() => {vm.GetAllFiles();});
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
            
            Assert.AreEqual(1, vm.AdditionalFiles.Count);
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
            Assert.AreEqual(2, vm.AdditionalFiles.Count);

            vm.RemoveItemCommand.Execute(pkgItem);
            Assert.AreEqual(1, vm.AdditionalFiles.Count);

            //arrange node libraries
            var assem = vm.Assemblies.FirstOrDefault().Assembly;
            var nodeLibraryNames = (IEnumerable<string>) new [] { assem.FullName };

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
            var newPkgVm = new PublishPackageViewModel(ViewModel) { CustomNodeDefinitions = new List<CustomNodeDefinition>(){ cnworkspace.CustomNodeDefinition } };
            newPkgVm.Name = "PublishingACustomNodeSetsPackageInfoCorrectly";
            newPkgVm.MajorVersion = "0";
            newPkgVm.MinorVersion = "0";
            newPkgVm.BuildVersion = "1";
            newPkgVm.PublishLocallyCommand.Execute();

            Assert.IsTrue(GetModel().GetPackageManagerExtension().PackageLoader.LocalPackages.Any
                (x => x.Name == "PublishingACustomNodeSetsPackageInfoCorrectly" && x.LoadState.State == PackageLoadState.StateTypes.Loaded && x.LoadedCustomNodes.Count ==1));


            Assert.AreEqual(new PackageInfo("PublishingACustomNodeSetsPackageInfoCorrectly", new Version(0,0,1))
                ,GetModel().CustomNodeManager.NodeInfos[cnworkspace.CustomNodeId].PackageInfo);
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
    }
}
