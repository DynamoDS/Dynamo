using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo;
using Dynamo.Core;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Dynamo.Wpf.Utilities;
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
        public void AddsFilesAndFoldersFromFilePathsCorrectly()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var allFolders = Directory.GetDirectories(nodePath, "*", SearchOption.AllDirectories).ToList();

            // Arrange
            var vm = new PublishPackageViewModel(this.ViewModel);
            vm.AddAllFilesAfterSelection(allFiles);

            var assemblies = vm.Assemblies;
            var additionalFiles = vm.AdditionalFiles;

            // Assert number of files PackageContents items is the one we expect
            Assert.AreEqual(assemblies.Count, allFiles.Count(f => f.EndsWith(".dll")));
            Assert.AreEqual(additionalFiles.Count, allFiles.Count(f => !f.EndsWith(".dll")));

            var packageContents = vm.PackageContents;
            Assert.AreEqual(packageContents.Count, 1); // We expect only 1 root item here

            // Assert that the PackageContents contains the correct number of items
            var allFilesAndFoldres = PackageItemRootViewModel.GetFiles(packageContents.First());
            Assert.AreEqual(allFilesAndFoldres.Count, allFiles.Count + allFolders.Count + 1);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Assembly)), assemblies.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), allFolders.Count + 1);

            // Arrange - try to add the same files again
            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            vm.AddAllFilesAfterSelection(allFiles);
            packageContents = vm.PackageContents;

            // Assert that the PackageContents still contains the correct number of items
            allFilesAndFoldres = PackageItemRootViewModel.GetFiles(packageContents.First());
            Assert.AreEqual(allFilesAndFoldres.Count, allFiles.Count + allFolders.Count + 1);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Assembly)), assemblies.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), allFolders.Count + 1);
        }

        [Test]
        public void RemoveMultipleRootItemsCorrectly()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            string duplicateNodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\DuplicatePackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var allDuplicateFiles = Directory.GetFiles(duplicateNodePath, "*", SearchOption.AllDirectories).ToList();

            // Arrange
            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var vm = new PublishPackageViewModel(this.ViewModel);
            vm.AddAllFilesAfterSelection(allFiles);
            vm.AddAllFilesAfterSelection(allDuplicateFiles);

            var packageContents = vm.PackageContents;
            Assert.AreEqual(packageContents.Count, 2); // We expect 2 separate root item here

            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(packageContents.First()));
        }


        [Test]
        public void AddsFilesAndFoldersFromMultipleFilePathsCorrectly()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            string duplicateNodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\DuplicatePackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var allFolders = Directory.GetDirectories(nodePath, "*", SearchOption.AllDirectories).ToList();
            var allDuplicateFiles = Directory.GetFiles(duplicateNodePath, "*", SearchOption.AllDirectories).ToList();
            var allDuplicateFolders = Directory.GetDirectories(duplicateNodePath, "*", SearchOption.AllDirectories).ToList();

            // Arrange
            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var vm = new PublishPackageViewModel(this.ViewModel);
            vm.AddAllFilesAfterSelection(allFiles);
            vm.AddAllFilesAfterSelection(allDuplicateFiles);

            var assemblies = vm.Assemblies;
            var additionalFiles = vm.AdditionalFiles;

            var packageContents = vm.PackageContents;
            Assert.AreEqual(packageContents.Count, 2); // We expect 2 separate root item here

            // Assert
            // that the PackageContents contains the correct number of items
            var allFilesAndFoldres = PackageItemRootViewModel.GetFiles(packageContents.ToList());

            // add 2 root folders, but discard one duplicate assembly file and one folder containing duplicate assembly file  
            Assert.AreEqual(allFilesAndFoldres.Count, allFiles.Count + allFolders.Count + (allDuplicateFiles.Count - 1) + (allDuplicateFolders.Count -1) + 2);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Assembly)), assemblies.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);

            // add 2 root folders, but discard one folder containing duplicate assembly file
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), allFolders.Count + (allDuplicateFolders.Count - 1) + 2);

            // Arrange
            // try to add the folder one level above the two root folders
            string commonRootPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder");
            var commonRootFiles = Directory.GetFiles(commonRootPath, "*", SearchOption.AllDirectories).ToList();
            var commonRootFolders = Directory.GetDirectories(commonRootPath, "*", SearchOption.AllDirectories).ToList();

            vm.AddAllFilesAfterSelection(commonRootFiles, commonRootPath);
            packageContents = vm.PackageContents;

            Assert.AreEqual(packageContents.Count, 1); // We expect only 1 common root item now

            // Assert
            // that the PackageContents still contains the correct number of items
            allFilesAndFoldres = PackageItemRootViewModel.GetFiles(packageContents.First());

            // add 1 root folder, but discard one duplicate assembly file and one folder containing duplicate assembly file  
            Assert.AreEqual(allFilesAndFoldres.Count, (commonRootFiles.Count - 1) + (commonRootFolders.Count - 1) + 1);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Assembly)), assemblies.Count);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);

            // add 1 root folder1, but discard one folder containing duplicate assembly file
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), (commonRootFolders.Count - 1) + 1);
        }


        [Test]
        public void GetAllFilesReturnsEqualsPackageContents()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            int packageContentsCount = 0;

            foreach(var rootItem in vm.PackageContents)
            {
                var items = PackageItemRootViewModel.GetFiles(rootItem).Where(x => !x.DependencyType.Equals(DependencyType.Folder)).ToList();
                packageContentsCount += items.Count;
            }

            var files = vm.GetAllFiles();

            // check if GetAllFiles return the same number of files as stored inside the PackageContents
            Assert.AreEqual(packageContentsCount, files.Count());
        }

        [Test]
        public void AssertGetPreBuildRootItemViewModelReturnsCorrectItem()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            var testPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\", "TestPath");
            var testPkgName = @"Test Package";

            var rootItemPreview = vm.GetPreBuildRootItemViewModel(testPath, testPkgName, allFiles);
            var allRootItems = PackageItemRootViewModel.GetFiles(rootItemPreview);

            var folders = allRootItems.Count(x => x.DependencyType.Equals(DependencyType.Folder));  
            var files = allRootItems.Count(x => !x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(5, folders);
            Assert.AreEqual(5, files);
        }


        [Test]
        public void AssertGetExistingRootItemViewModelReturnsCorrectItem()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            var testPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\", "TestPath");
            var testPkgName = @"Test Package";

            // We expect a single root item for this test
            var rootItem = vm.PackageContents.First();
            var allRootItems = PackageItemRootViewModel.GetFiles(vm.PackageContents.First());

            var rootItemPreview = vm.GetExistingRootItemViewModel(testPath, testPkgName);
            var testRootItems = PackageItemRootViewModel.GetFiles(rootItemPreview);


            Assert.AreNotEqual(testPkgName, rootItem.DisplayName);
            Assert.AreEqual(testPkgName, rootItemPreview.DisplayName);

            var foldersCount = allRootItems.Count(x => x.DependencyType.Equals(DependencyType.Folder));
            var filesCount = allRootItems.Count(x => !x.DependencyType.Equals(DependencyType.Folder));

            var testFoldersCount = testRootItems.Count(x => x.DependencyType.Equals(DependencyType.Folder));
            var testFilesCount = testRootItems.Count(x => !x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(foldersCount, testFoldersCount);
            Assert.AreEqual(filesCount, testFilesCount);
        }
                

        [Test]
        public void RemoveFilesUpdatesPerviewContentItem()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            Assert.AreEqual(1, vm.PackageContents.Count);
            Assert.AreEqual(1, vm.PreviewPackageContents.Count);
            var rootItem = vm.PackageContents.First();


            // Assert
            vm.RemoveItemCommand.Execute(rootItem);
            Assert.AreEqual(0, vm.PackageContents.Count);
            Assert.AreEqual(0, vm.PreviewPackageContents.Count);
        }


        [Test]
        public void RemoveAllChildrenFilesUpdatesContentItem()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            Assert.AreEqual(1, vm.PackageContents.Count);
            Assert.AreEqual(1, vm.PreviewPackageContents.Count);
            var childItems = vm.PackageContents.First().ChildItems;

            // Assert
            foreach(var child in childItems)
            {
                vm.RemoveItemCommand.Execute(child);
            }

            Assert.AreEqual(0, vm.PackageContents.Count);
            Assert.AreEqual(0, vm.PreviewPackageContents.Count);
        }


        [Test]
        public void CanRemoveCustomDefinitionDependencyTypes()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\AllFileTypesPackageDocs");
            string dyfPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\AllFileTypesPackageDocs\\dyf\\3DView by BoundingBox.dyf");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            // One root folder, one custom definition 
            Assert.AreEqual(2, vm.PackageContents.Count);

            var customDefinition = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.CustomNode));
            var rootFolder = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(1, customDefinition.Count());
            Assert.AreEqual(1, rootFolder.Count());

            var childItems = PackageItemRootViewModel.GetFiles(rootFolder.First());
            var customPreviewDefinition = childItems.Where(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));

            Assert.AreEqual(1, customPreviewDefinition.Count());

            // Assert
            // Remove using the custom definition
            vm.RemoveItemCommand.Execute(customDefinition.First());

            var updatedCustomDefinitionCount = vm.PackageContents.Count(x => x.DependencyType.Equals(DependencyType.CustomNode));
            var updatedCustomPreviewDefinitionCount = PackageItemRootViewModel.GetFiles(rootFolder.First()).
                Count(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));

            Assert.AreEqual(0, updatedCustomDefinitionCount, updatedCustomPreviewDefinitionCount);

            // Add 
            vm.AddAllFilesAfterSelection(new List<string>() { dyfPath });

            updatedCustomDefinitionCount = vm.PackageContents.Count(x => x.DependencyType.Equals(DependencyType.CustomNode));
            updatedCustomPreviewDefinitionCount = PackageItemRootViewModel.GetFiles(rootFolder.First()).
                Count(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));

            Assert.AreEqual(1, updatedCustomDefinitionCount, updatedCustomPreviewDefinitionCount);

            // Remove using the preview
            vm.RemoveItemCommand.Execute(customPreviewDefinition.First());

            updatedCustomDefinitionCount = vm.PackageContents.Count(x => x.DependencyType.Equals(DependencyType.CustomNode));
            updatedCustomPreviewDefinitionCount = PackageItemRootViewModel.GetFiles(rootFolder.First()).
                Count(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));

            Assert.AreEqual(0, updatedCustomDefinitionCount, updatedCustomPreviewDefinitionCount);
        }

        [Test]
        public void CanRemoveAllDependencyTypes()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\AllFileTypesPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            var rootFolder = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(1, rootFolder.Count());

            var childItems = PackageItemRootViewModel.GetFiles(rootFolder.First());

            var files = childItems.Where(x => x.DependencyType.Equals(DependencyType.File));
            var dllFiles = childItems.Where(x => x.DependencyType.Equals(DependencyType.Assembly));
            var dyfPreviewFiles = childItems.Where(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));
            var folders = childItems.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(5, files.Count());
            Assert.AreEqual(1, dllFiles.Count());
            Assert.AreEqual(1, dyfPreviewFiles.Count());
            Assert.AreEqual(5, folders.Count());

            // Assert
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(dllFiles.First()));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(dyfPreviewFiles.First()));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(files.First(x => x.DisplayName.EndsWith(".json"))));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(files.First(x => x.DisplayName.EndsWith(".xml"))));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(files.First(x => x.DisplayName.EndsWith(".dll"))));

            // At this point, only one root item remains, not the original one but the 'doc' folder
            // The original root item no longer contains a file, therefore it was removed
            // This makes sense as we don't want to try to establish 'common parent' for folders that maybe too far apart in a tree structure
            rootFolder = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.Folder));
            Assert.AreEqual(1, rootFolder.Count());
            Assert.AreEqual(3, PackageItemRootViewModel.GetFiles(rootFolder.First()).Count());

            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(rootFolder.First()));
            Assert.IsFalse(vm.PackageContents.Any());
        }


        [Test]
        public void CancelCommandClearsAllData()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            Assert.AreEqual(0, vm.PackageContents.Count);
            Assert.AreEqual(0, vm.PreviewPackageContents.Count);

            ViewModel.OnRequestPackagePublishDialog(vm);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            Assert.AreEqual(1, vm.PackageContents.Count);
            Assert.AreEqual(1, vm.PreviewPackageContents.Count);
            
            vm.CancelCommand.Execute();

            // Assert
            Assert.AreEqual(0, vm.PackageContents.Count);
            Assert.AreEqual(0, vm.PreviewPackageContents.Count);
        }

        [Test]
        public void AssertPreviewPackageDefaultFolderStructureEqualsPublishLocalPackageResults()
        {
            var packageName = "SingleFolderPublishPackage";
            var pathManager = this.ViewModel.Model.PathManager as PathManager;
            var publishPath = Path.Combine(pathManager.DefaultPackagesDirectory, packageName);

            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            //now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);

            ViewModel.OnRequestPackagePublishDialog(newPkgVm);

            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));
            var prDllFiles = previewFiles.Where(x => x.DisplayName.EndsWith(".dll"));

            Assert.IsTrue(prDllFiles.All(x => Path.GetDirectoryName(x.FilePath) == Path.GetDirectoryName(prDllFiles.First().FilePath)));
            var prDllFolder = Path.GetDirectoryName(prDllFiles.First().FilePath);

            newPkgVm.Name = "SingleFolderPublishPackage";
            newPkgVm.MajorVersion = "0";
            newPkgVm.MinorVersion = "0";
            newPkgVm.BuildVersion = "1";
            newPkgVm.PublishLocallyCommand.Execute();

            Assert.IsTrue(Directory.Exists(publishPath));

            // Arrange
            var createdFiles = Directory.GetFiles(publishPath, "*", SearchOption.AllDirectories).ToList();
            var createdFolders = Directory.GetDirectories(publishPath, "*", SearchOption.AllDirectories).ToList();
            var crDllFiles = createdFiles.Where(x => x.EndsWith(".dll"));

            Assert.IsTrue(crDllFiles.All(x => Path.GetDirectoryName(x) == Path.GetDirectoryName(crDllFiles.First())));
            var crDllFolder = Path.GetDirectoryName(crDllFiles.First());

            // Assert
            Assert.AreEqual(createdFiles.Count(), previewFiles.Count());
            Assert.AreEqual(createdFolders.Count(), previewFolders.Count() - 1);  // discount one for the root folder is included
            Assert.AreEqual(2, crDllFiles.Count(), prDllFiles.Count());  
            Assert.AreEqual(Path.GetFileName(crDllFolder), Path.GetFileName(prDllFolder));  // check if the dll parent folder is the same

            // Clean up
            Directory.Delete(publishPath, true);
        }

        [Test]
        public void AssertPreviewPackageRetainFolderStructureEqualsPublishLocalPackageResults()
        {
            var packageName = "SingleFolderPublishPackage";
            var pathManager = this.ViewModel.Model.PathManager as PathManager;
            var publishPath = Path.Combine(pathManager.DefaultPackagesDirectory, packageName);

            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            //now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);
            newPkgVm.RetainFolderStructureOverride = true;

            ViewModel.OnRequestPackagePublishDialog(newPkgVm);

            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            newPkgVm.Name = "SingleFolderPublishPackage";
            newPkgVm.MajorVersion = "0";
            newPkgVm.MinorVersion = "0";
            newPkgVm.BuildVersion = "1";
            newPkgVm.PublishLocallyCommand.Execute();

            Assert.IsTrue(Directory.Exists(publishPath));

            // Arrange
            var createdFiles = Directory.GetFiles(publishPath, "*", SearchOption.AllDirectories).ToList();
            var createdFolders = Directory.GetDirectories(publishPath, "*", SearchOption.AllDirectories).ToList();

            // Assert
            Assert.AreEqual(createdFiles.Count(), previewFiles.Count() + 1);
            Assert.AreEqual(1, createdFolders.Count(), previewFolders.Count());  // One subfolder was created

            // Clean up
            Directory.Delete(publishPath, true);
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
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package);
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
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package);
            });

            Assert.AreEqual(PackageUploadHandle.State.Error, vm.UploadState);
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
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package);
            });

            vm.AddFile(addFilePath);
            Assert.AreEqual(1, vm.AdditionalFiles.Count);

            vm.RemoveItemCommand.Execute(pkgItem);
            Assert.AreEqual(0, vm.AdditionalFiles.Count);
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


        [Test]
        public void AssertPublishLocalHandleType()
        {
            var packageName = "SingleFolderPublishPackage";
            var pathManager = this.ViewModel.Model.PathManager as PathManager;
            var publishPath = Path.Combine(pathManager.DefaultPackagesDirectory, packageName);

            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            //now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);
            newPkgVm.RetainFolderStructureOverride = true;

            ViewModel.OnRequestPackagePublishDialog(newPkgVm);

            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            newPkgVm.Name = "SingleFolderPublishPackage";
            newPkgVm.MajorVersion = "0";
            newPkgVm.MinorVersion = "0";
            newPkgVm.BuildVersion = "1";
            newPkgVm.PublishLocallyCommand.Execute();

            // Assert
            Assert.AreEqual(PackageUploadHandle.UploadType.Local, newPkgVm.UploadType);

            // Clean up
            Directory.Delete(publishPath, true);
        }

    }
}
