using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Moq;
using NUnit.Framework;
using SystemTestServices;


namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PackageManagerUITests : SystemTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "pkgs"); } }
        public string PackagesDirectorySigned { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "pkgs_signed"); } }
        internal string StandardLibraryTestDirectory { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "standard lib testdir", "Packages"); } }

        #region Utility functions

        protected void LoadPackage(string packageDirectory)
        {
            Model.PreferenceSettings.CustomPackageFolders.Add(packageDirectory);
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> { pkg });
        }

        protected PackageLoader GetPackageLoader()
        {
            var extensions = Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>();
            if (extensions.Any())
            {
                return extensions.First().PackageLoader;
            }

            return null;
        }

        public IEnumerable<Window> GetWindowEnumerable(WindowCollection windows)
        {
            var enumerator = windows.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return (Window)enumerator.Current;
            }
        }

        public void AssertWindowOwnedByDynamoView<T>()
        {
            var windows = GetWindowEnumerable(View.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)View);
        }

        public void AssertWindowClosedWithDynamoView<T>()
        {
            var windows = GetWindowEnumerable(View.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)View);
        }

        public override void Setup()
        {
            base.Setup();
            ViewModel.PreferenceSettings.PackageDownloadTouAccepted = true;
        }

        #endregion

        #region PackageManagerPublishView

        [Test]
        public void CanOpenPackagePublishDialogAndWindowIsOwned()
        {
            var l = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PublishPackageView>();
        }

        [Test,Ignore]
        public void CannotCreateDuplicatePackagePublishDialogs()
        {
            var l = new PublishPackageViewModel(ViewModel);
            for (var i = 0; i < 10; i++)
            {
                ViewModel.OnRequestPackagePublishDialog(l);
            }

            AssertWindowOwnedByDynamoView<PublishPackageView>();
        }

        [Test]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PublishPackageView>();
            AssertWindowClosedWithDynamoView<PublishPackageView>();

        }
        #endregion

        #region InstalledPackagesView

        [Test]
        public void CanOpenManagePackagesDialogAndWindowIsOwned()
        {
            ViewModel.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        //[Test, Ignore]
        //public void CannotCreateDuplicateManagePackagesDialogs()
        //{
        //    for (var i = 0; i < 10; i++)
        //    {
        //        Vm.OnRequestManagePackagesDialog(null, null);
        //    }

        //    AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        //}

        [Test]
        public void ManagePackagesDialogClosesWithDynamo()
        {
            ViewModel.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
            AssertWindowClosedWithDynamoView<InstalledPackagesView>();

        }

        [Test]
        [Description("User tries to download pacakges that might conflict with existing packages in std lib")]
        public void PackageManagerConflictsWithStdLib()
        {
            var pkgLoader = GetPackageLoader();
            pkgLoader.StandardLibraryDirectory = StandardLibraryTestDirectory;

            // Load a std lib package
            var stdPackageLocation = Path.Combine(StandardLibraryTestDirectory, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(stdPackageLocation);

            var stdLibPkg = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(stdLibPkg);

            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            //
            // 1. User downloads the exact version of a std lib package
            //
            {
                var id = "test-123";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = stdLibPkg.Name } };
                var depVers = new List<string>() { stdLibPkg.VersionName };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = stdLibPkg.VersionName,
                        engine_version = stdLibPkg.EngineVersion,
                        name = stdLibPkg.Name,
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(stdLibPkg.Name, VersionUtilities.PartialParse(stdLibPkg.VersionName));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 2 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with the same name and version already exists as part of the Standard Library.
                dlgMock.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
                dlgMock.ResetCalls();
            }

            //
            // 2. User downloads a different version of a std lib package
            //
            {
                var id = "test-234";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = stdLibPkg.Name } };
                var stdLibPkgVers = VersionUtilities.PartialParse(stdLibPkg.VersionName);
                var newPkgVers = new Version(stdLibPkgVers.Major + 1, stdLibPkgVers.Minor, stdLibPkgVers.Build);

                var depVers = new List<string>() { newPkgVers.ToString() };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = newPkgVers.ToString(),
                        engine_version = stdLibPkg.EngineVersion,
                        name = stdLibPkg.Name,
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });
                
                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(stdLibPkg.Name, newPkgVers);
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 2 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with a different version already exists as part of the Standard Library.
                dlgMock.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
                dlgMock.ResetCalls();
            }

            //
            // 3. User downloads a package that is not part of a std lib package
            //
            {
                var id = "test-345";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = "non-std-libg" } };
                var pkgVersion = new Version(1, 0 ,0);
                var depVers = new List<string>() { pkgVersion.ToString() };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = pkgVersion.ToString(),
                        engine_version = stdLibPkg.EngineVersion,
                        name = "non-std-libg",
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo("Non-stdlib-package", new Version(1, 0, 0));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 1 warning :
                // 1. To confirm that they want to download the specified package.
                dlgMock.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
                dlgMock.ResetCalls();
            }
        }

        #endregion

        #region PackageManagerSearchView

        [Test]
        public void CanOpenPackageSearchDialogAndWindowIsOwned()
        {
            ViewModel.OnRequestPackageManagerSearchDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void CannotCreateDuplicatePackageSearchDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                ViewModel.OnRequestPackageManagerSearchDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void PackageSearchDialogClosesWithDynamo()
        {
            ViewModel.OnRequestPackageManagerSearchDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
            AssertWindowClosedWithDynamoView<PackageManagerSearchView>();

        }

        [Test]
        public void PackageSearchDialogSearchTextCollapsedWhileSyncing()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Syncing;

            // Assert
            Assert.AreEqual(false, searchViewModel.ShowSearchText);
        }

        [Test]
        public void PackageSearchDialogSearchTextVisibleWithResults()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Results;

            // Assert
            Assert.AreEqual(true, searchViewModel.ShowSearchText);
        }

        [Test]
        public void PackageSearchDialogSearchTextVisibleWhenSearching()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Searching;

            // Assert
            Assert.AreEqual(true, searchViewModel.ShowSearchText);
        }

        [Test]
        public void PackageSearchDialogSearchTextVisibleWhenNoResults()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.NoResults;

            // Assert
            Assert.AreEqual(true, searchViewModel.ShowSearchText);
        }

        [Test]
        public void PackageSearchDialogSearchBoxPromptTextWhileSyncing()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Syncing;

            // Assert
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.PackageSearchViewSearchTextBoxSyncing, searchViewModel.SearchBoxPrompt);
        }

        [Test]
        public void PackageSearchDialogSearchBoxPromptTextWhenNotSyncing()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Results;

            // Assert
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.PackageSearchViewSearchTextBox, searchViewModel.SearchBoxPrompt);
        }

        [Test]
        public void PackageManagerCrashTestOnDownloadingInvalidPackage()
        {
            string packageDirectory = Path.Combine(GetTestDirectory(ExecutingDirectory), @"pkgs\Autodesk Steel Package");

            try
            {
                LoadPackage(packageDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load the package: "+ e);
            }

            var loader = GetPackageLoader();
            var packageFound = loader.LocalPackages.Any(x => x.Name == "Autodesk Steel Connections 2020");
            Assert.IsFalse(packageFound);

            ViewModel.OnRequestManagePackagesDialog(null, null);
            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        #endregion

    }
}
