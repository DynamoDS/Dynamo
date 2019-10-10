using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PublishPackageViewModelTests: DynamoViewModelUnitTest
    {

        [Test, Category("Failure")]
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
        public void NewPackageVersionUpload_DoesNotThrowExceptionWhenDLLIsLoadedSeveralTimes()
        {
            string packagesDirectory = Path.Combine(TestDirectory, "pkgs");

            var loader = new PackageLoader(packagesDirectory);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings
            });

            PublishPackageViewModel vm = null;
            var package = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package);
            });

            Assert.AreEqual(PackageUploadHandle.State.Error, vm.UploadState);
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
                (x => x.Name == "PublishingACustomNodeSetsPackageInfoCorrectly" && x.Loaded == true && x.LoadedCustomNodes.Count ==1));


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
    }
}
