using NUnit.Framework;
using System.Linq;
using Dynamo.PackageManager;
using System.IO;
using Dynamo.Tests;
using System;
using Moq;
using System.Collections.Generic;
using Dynamo.Graph.Workspaces;

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
            var first = Path.GetFullPath(packages.First());
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
        public void NewPackageVersionUpload_DoesNotThrowExceptionWhenDLLIsLoadedSeveralTimes()
        {
            string packagesDirectory = Path.Combine(TestDirectory, "pkgs");

            var loader = new PackageLoader(packagesDirectory);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings
            });

            PublishPackageViewModel vm = null;
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, loader.LocalPackages.First());
            });

            Assert.AreEqual(PackageUploadHandle.State.Error, vm.UploadState);
        }

    }
}
