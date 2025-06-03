using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.Interfaces;
using Dynamo.PackageManager.Tests;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.Views;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Moq;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests.PackageManager
{
    [TestFixture]
    public class PackageManagerUITests : SystemTestBase
    {
        internal string TestDirectory { get { return GetTestDirectory(ExecutingDirectory); } }
        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }
        public string PackagesDirectorySigned { get { return Path.Combine(TestDirectory, "pkgs_signed"); } }

        internal string BuiltinPackagesTestDir { get { return Path.Combine(TestDirectory, "builtinpackages testdir", "Packages"); } }

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
        public static bool ComparePaths(string path1, string path2)
        {
            return PackageDirectoryBuilder.NormalizePath(path1) == PackageDirectoryBuilder.NormalizePath(path2);
        }

        public void AssertWindowOwnedByDynamoView<T>()
        {
            var windows = GetWindowEnumerable(View.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == View);
        }

        public void AssertWindowClosedWithDynamoView<T>()
        {
            var windows = GetWindowEnumerable(View.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == View);
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

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void CannotCreateDuplicatePackagePublishDialogs()
        {
            var l = new PublishPackageViewModel(ViewModel);
            for (var i = 0; i < 10; i++)
            {
                ViewModel.OnRequestPackagePublishDialog(l);
            }

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerView>();
            AssertWindowClosedWithDynamoView<PackageManagerView>();

        }

        [Test]
        public void PackagePublishKeywordPropertyAllowsSpaces()
        {
            // Arrange
            var singleSpace = " ";
            var doubleSpace = "  ";
            var singleWord = "support";
            var doubleWord = "support mep";
            var doubleWordMultipleSpaces = "support    mep";
            var commaDelimetedWords = "support,mep";

            var l = new PublishPackageViewModel(ViewModel);

            // Act/Assert
            l.Keywords = singleSpace;
            Assert.AreEqual(singleSpace, l.Keywords);
            Assert.AreEqual(0, l.KeywordList.Count());

            l.Keywords = doubleSpace;
            Assert.AreEqual(singleSpace, l.Keywords);
            Assert.AreEqual(0, l.KeywordList.Count());

            l.Keywords = singleWord;
            Assert.AreEqual(singleWord, l.Keywords);
            Assert.AreEqual(1, l.KeywordList.Count());

            l.Keywords = doubleWord;
            Assert.AreEqual(doubleWord, l.Keywords);
            Assert.AreEqual(2, l.KeywordList.Count());

            l.Keywords = doubleWordMultipleSpaces;
            Assert.AreEqual(doubleWord, l.Keywords);
            Assert.AreEqual(2, l.KeywordList.Count());

            l.Keywords = commaDelimetedWords;
            Assert.AreEqual(commaDelimetedWords.Replace(',', ' '), l.Keywords);
            Assert.AreEqual(2, l.KeywordList.Count());
        }

        [Test]
        public void PackagePublishNumericUpDownAllowsOnlyPositiveIntegerValues()
        {
            // Arrange
            var updownControl = new NumericUpDownControl();

            // Assert negavie
            var isValidOutput = updownControl.IsValidInput("l");
            Assert.IsFalse(isValidOutput, "Should not allow letters in numeric input.");

            isValidOutput = updownControl.IsValidInput("-1");
            Assert.IsFalse(isValidOutput, "Should not allow negative integers in numeric input.");

            isValidOutput = updownControl.IsValidInput("1.0");
            Assert.IsFalse(isValidOutput, "Should not allow fractional input.");

            isValidOutput = updownControl.IsValidInput(" ");
            Assert.IsFalse(isValidOutput, "Should not allow space input values.");

            isValidOutput = updownControl.IsValidInput(" 1");
            Assert.IsFalse(isValidOutput, "Should not allow space input values.");

            // Assert positive
            isValidOutput = updownControl.IsValidInput("0");
            Assert.IsTrue(isValidOutput, "Should allow 0 in numeric input.");

            isValidOutput = updownControl.IsValidInput("1");
            Assert.IsTrue(isValidOutput, "Should allow positive integers in numeric input.");

            // Arrange
            var inputField = updownControl.inputField;

            // Act & Assert
            Assert.AreEqual("0", updownControl.Value);

            inputField.Text = "1";
            Assert.AreEqual("1", updownControl.Value);

            inputField.Text = "";
            Assert.AreEqual("0", updownControl.Value);
        }

        [Test]
        public void TestAscendingSpinnerClickEvent()
        {
            // Arrange
            var updownControl = new NumericUpDownControl();
            var up = updownControl.spinnerUp;
            var down = updownControl.spinnerDown;
            var inputField = updownControl.inputField;
            var watermark = updownControl.Watermark;

            // Act
            watermark = "0";
            up.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            // Assert
            Assert.AreEqual(inputField.Text, "1");

            up.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "2");

            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "1");

            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "0");

            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "0");
        }

        [Test]
        public void TestDescendingSpinnerClickEvent()
        {
            // Arrange
            var updownControl = new NumericUpDownControl();
            var up = updownControl.spinnerUp;
            var down = updownControl.spinnerDown;
            var inputField = updownControl.inputField;
            var watermark = updownControl.Watermark;

            // Act
            watermark = "1";
            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            // Assert
            Assert.AreEqual(inputField.Text, "0");

            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "0");

            up.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "1");

            up.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "2");

            down.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(inputField.Text, "1");
        }
        #endregion

        #region InstalledPackagesControl

        [Test]
        public void CanOpenManagePackagesDialogAndWindowIsOwned()
        {
            var preferencesWindow = new PreferencesView(View)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            preferencesWindow.Show();

            AssertWindowOwnedByDynamoView<PreferencesView>();
        }

        [Test]
        public void ManagePackagesDialogClosesWithDynamo()
        {
            var preferencesWindow = new PreferencesView(View)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            preferencesWindow.Show();

            AssertWindowOwnedByDynamoView<PreferencesView>();
            AssertWindowClosedWithDynamoView<PreferencesView>();
        }

        [Test]
        [Description("User tries to download packages that might conflict with existing packages in builtIn")]
        public void PackageManagerConflictsWithbltinpackages()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            // Load a builtIn package
            var builtInPackageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(builtInPackageLocation);

            var bltInPackage = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(bltInPackage);

            bltInPackage.SetAsLoaded();

            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            //
            // 1. User downloads the exact version of a builtIn package
            //
            {
                var id = "test-123";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = bltInPackage.Name } };
                var depVers = new List<string>() { bltInPackage.VersionName };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = bltInPackage.VersionName,
                        engine_version = bltInPackage.EngineVersion,
                        name = bltInPackage.Name,
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(bltInPackage.Name, VersionUtilities.PartialParse(bltInPackage.VersionName));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 2 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with the same name and version already exists as part of the BuiltinPackages.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
                dlgMock.ResetCalls();
            }

            //
            // 2. User downloads a different version of a builtIn package
            //
            {
                var id = "test-234";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = bltInPackage.Name } };
                var bltinpackagesPkgVers = VersionUtilities.PartialParse(bltInPackage.VersionName);
                var newPkgVers = new Version(bltinpackagesPkgVers.Major + 1, bltinpackagesPkgVers.Minor, bltinpackagesPkgVers.Build);

                var depVers = new List<string>() { newPkgVers.ToString() };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = newPkgVers.ToString(),
                        engine_version = bltInPackage.EngineVersion,
                        name = bltInPackage.Name,
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(bltInPackage.Name, newPkgVers);
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 2 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with a different version already exists as part of the BuiltinPackages.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
                dlgMock.ResetCalls();
            }

            //
            // 3. User downloads a package that is not part of a builtin pkgs.
            //
            {
                var id = "test-345";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = "non-builtin-libg" } };
                var pkgVersion = new Version(1, 0, 0);
                var depVers = new List<string>() { pkgVersion.ToString() };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = pkgVersion.ToString(),
                        engine_version = bltInPackage.EngineVersion,
                        name = "non-builtin-libg",
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo("Non-builtin-package", new Version(1, 0, 0));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 1 warning :
                // 1. To confirm that they want to download the specified package.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
                dlgMock.ResetCalls();
            }
        }

        [Test]
        [Description("User tries to download a package that has a dependency that matches existing built-in package")]
        public void PackageManagerDepMatchesBltinPackage()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            // Load a builtIn package
            var builtInPackageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(builtInPackageLocation);

            var bltInPackage = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(bltInPackage);

            // Simulate the user downloading the a package from PM that has a dependency of the same name
            // as the installed built-in package.
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var name = "test-123";
            var deps = new List<Dependency>() { new Dependency {_id = name, name = name},
                new Dependency {_id = bltInPackage.Name, name = bltInPackage.Name} };
            var version = "1.0.0";

            // User downloads a package that has a dependency on the same version of the builtIn package
            // that is locally installled
            var depVersion = "1.0.0";
            var depVers = new List<string>() { version, depVersion };

            // IGregClient.ExecuteAndDeserializeWithContent() is called thrice, twice for the package being downloaded
            // and once for its package dependency, which is why it needs to be mocked thrice in sequence.
            mockGreg.SetupSequence(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
            .Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = bltInPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = bltInPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = depVersion,
                    engine_version = bltInPackage.EngineVersion,
                    name = bltInPackage.Name,
                    id = bltInPackage.Name,
                    full_dependency_ids = new List<Dependency>(),
                    full_dependency_versions = new List<string>()
                },
                success = true
            });

            var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(name, VersionUtilities.PartialParse(version));
            pmVm.DownloadAndInstallPackage(pkgInfo);

            // Users should get 1 warning only:
            // 1. To confirm that they want to download the specified package.

            // Since the depedency is of the same version as the built-in package installed,
            // there is no further warning message, and the package will download without further user input.
            dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
            dlgMock.ResetCalls();
        }

        [Test]
        [Description("User tries to download a package that has a dependency that conflicts with existing built-in package")]
        public void PackageManagerDepConflictsWithBltinPackage()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            // Load a builtIn package
            var builtInPackageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(builtInPackageLocation);

            var bltInPackage = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(bltInPackage);

            bltInPackage.SetAsLoaded();

            // Simulate the user downloading the a package from PM that has a dependency of the same name
            // as the installed built-in package.
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var name = "test-123";
            var deps = new List<Dependency>() { new Dependency {_id = name, name = name},
                new Dependency {_id = bltInPackage.Name, name = bltInPackage.Name} };
            var version = "1.0.0";

            // User downloads a package that has a dependency on a different version of the builtIn package 
            // than the one locally installed.
            var depVersion = "1.9.0";
            var depVers = new List<string>() { version, depVersion };

            // IGregClient.ExecuteAndDeserializeWithContent() is called thrice, twice for the package being downloaded
            // and once for its package dependency, which is why it needs to be mocked thrice in sequence.
            mockGreg.SetupSequence(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
            .Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = bltInPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = bltInPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = depVersion,
                    engine_version = bltInPackage.EngineVersion,
                    name = bltInPackage.Name,
                    id = bltInPackage.Name,
                    full_dependency_ids = new List<Dependency>(),
                    full_dependency_versions = new List<string>()
                },
                success = true
            });

            var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(name, VersionUtilities.PartialParse(version));
            pmVm.DownloadAndInstallPackage(pkgInfo);

            // Users should get 2 warnings:
            // 1. To confirm that they want to download the specified package.
            // 2. To warn of the built-in package conflict (same name, diff version)
            dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
            dlgMock.ResetCalls();
        }

        [Test]
        [Description("User tries to download a package that has a dependency that matches existing package")]
        public void PackageManagerDepMatchesLocalPackage()
        {
            var pkgLoader = GetPackageLoader();

            // Load a package
            string packageLocation = Path.Combine(GetTestDirectory(ExecutingDirectory), @"pkgs\Package");
            pkgLoader.ScanPackageDirectory(packageLocation);

            var localPkg = pkgLoader.LocalPackages.Where(x => x.Name == "Package").FirstOrDefault();
            Assert.IsNotNull(localPkg);

            // Simulate the user downloading the a package from PM that has a dependency of the same name
            // as the installed built-in package.
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var name = "test-123";
            var deps = new List<Dependency>() { new Dependency {_id = name, name = name},
                new Dependency {_id = localPkg.Name, name = localPkg.Name} };
            var version = "1.0.0";

            // User downloads a package that has a dependency on the same version of the builtIn package
            // that is locally installled
            var depVersion = "1.0.0";
            var depVers = new List<string>() { version, depVersion };

            // IGregClient.ExecuteAndDeserializeWithContent() is called thrice, twice for the package being downloaded
            // and once for its package dependency, which is why it needs to be mocked thrice in sequence.
            mockGreg.SetupSequence(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
            .Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = localPkg.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = localPkg.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = depVersion,
                    engine_version = localPkg.EngineVersion,
                    name = localPkg.Name,
                    id = localPkg.Name,
                    full_dependency_ids = new List<Dependency>(),
                    full_dependency_versions = new List<string>()
                },
                success = true
            });

            var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(name, VersionUtilities.PartialParse(version));
            pmVm.DownloadAndInstallPackage(pkgInfo);

            // Users should get 1 warning only:
            // 1. To confirm that they want to download the specified package.

            // Since the depedency is of the same version as the local package installed,
            // there is no further warning message, and the package will download without further user input.
            dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
            dlgMock.ResetCalls();
        }

        [Test]
        [Description("User tries to download a package that has a dependency that conflicts with existing package")]
        public void PackageManagerDepConflictsWithLocalPackage()
        {
            var pkgLoader = GetPackageLoader();

            // Load a package
            string packageLocation = Path.Combine(GetTestDirectory(ExecutingDirectory), @"pkgs\Package");
            var newpkg = pkgLoader.ScanPackageDirectory(packageLocation);
            newpkg?.SetAsLoaded();

            var localPackage = pkgLoader.LocalPackages.Where(x => x.Name == "Package").FirstOrDefault();
            Assert.IsNotNull(localPackage);

            // Simulate the user downloading the a package from PM that has a dependency of the same name
            // as the installed built-in package.
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var name = "test-123";
            var deps = new List<Dependency>() { new Dependency {_id = name, name = name},
                new Dependency {_id = localPackage.Name, name = localPackage.Name} };
            var version = "1.0.0";

            // User downloads a package that has a dependency on a different version of the builtIn package 
            // than the one locally installed.
            var depVersion = "1.9.0";
            var depVers = new List<string>() { version, depVersion };

            // IGregClient.ExecuteAndDeserializeWithContent() is called thrice, twice for the package being downloaded
            // and once for its package dependency, which is why it needs to be mocked thrice in sequence.
            mockGreg.SetupSequence(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
            .Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = localPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = version,
                    engine_version = localPackage.EngineVersion,
                    name = name,
                    id = name,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                },
                success = true
            }).Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = new PackageVersion()
                {
                    version = depVersion,
                    engine_version = localPackage.EngineVersion,
                    name = localPackage.Name,
                    id = localPackage.Name,
                    full_dependency_ids = new List<Dependency>(),
                    full_dependency_versions = new List<string>()
                },
                success = true
            });

            var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(name, VersionUtilities.PartialParse(version));
            pmVm.DownloadAndInstallPackage(pkgInfo);

            // Users should get 2 warnings:
            // 1. To confirm that they want to download the specified package.
            // 2. To warn of the package conflict (same name, diff version)
            dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(2));
            dlgMock.ResetCalls();
        }

        [Test]
        [Description("DynamoMessageBox converts button inputs correctly.")]
        public void DynamoMessageBoxButtonCases()
        {
            var messageBox = new DynamoMessageBox();

            messageBox.ConfigureButtons(MessageBoxButton.YesNoCancel);
            Assert.AreEqual("Yes", messageBox.YesButton.Content);
            Assert.AreEqual("No", messageBox.NoButton.Content);
            Assert.AreEqual("Cancel", messageBox.CancelButton.Content);

            messageBox.ConfigureButtons(MessageBoxButton.YesNoCancel, new string[] { "continue", "unload", "cancel" });
            Assert.AreEqual("continue", messageBox.YesButton.Content);
            Assert.AreEqual("unload", messageBox.NoButton.Content);
            Assert.AreEqual("cancel", messageBox.CancelButton.Content);

            messageBox = new DynamoMessageBox();

            messageBox.ConfigureButtons(MessageBoxButton.OK);
            Assert.AreEqual("OK", messageBox.OkButton.Content);

            messageBox.ConfigureButtons(MessageBoxButton.OK, new string[] { "ok2" });
            Assert.AreEqual("ok2", messageBox.OkButton.Content);

            messageBox = new DynamoMessageBox();

            messageBox.ConfigureButtons(MessageBoxButton.OKCancel);
            Assert.AreEqual("OK", messageBox.OkButton.Content);
            Assert.AreEqual("Cancel", messageBox.CancelButton.Content);

            messageBox.ConfigureButtons(MessageBoxButton.OKCancel, new string[] { "1", "2" });
            Assert.AreEqual("1", messageBox.OkButton.Content);
            Assert.AreEqual("2", messageBox.CancelButton.Content);

            messageBox = new DynamoMessageBox();

            messageBox.ConfigureButtons(MessageBoxButton.YesNo);
            Assert.AreEqual("Yes", messageBox.YesButton.Content);
            Assert.AreEqual("No", messageBox.NoButton.Content);

            messageBox.ConfigureButtons(MessageBoxButton.YesNo, new string[] { "1", "2" });
            Assert.AreEqual("1", messageBox.YesButton.Content);
            Assert.AreEqual("2", messageBox.NoButton.Content);
        }

        [Test]
        [Description("DynamoMessageBox buttons set correct results")]
        public void DynamoMessageBoxButtonsSetCorrectly()
        {
            var messageBox = new DynamoMessageBox();
            messageBox.YesButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(MessageBoxResult.Yes, messageBox.CustomDialogResult);

            messageBox.NoButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(MessageBoxResult.No, messageBox.CustomDialogResult);

            messageBox.CancelButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(MessageBoxResult.Cancel, messageBox.CustomDialogResult);

            messageBox.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(MessageBoxResult.None, messageBox.CustomDialogResult);

            messageBox.OkButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(MessageBoxResult.OK, messageBox.CustomDialogResult);
        }



        [Test]
        [Description("User tries to load an unloaded built-in package")]
        public void PackageManagerLoadBuiltIn()
        {
            var currentDynamoModel = ViewModel.Model;
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            currentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>() { PackagesDirectorySigned };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = currentDynamoModel.PreferenceSettings,
            };
            var loader = currentDynamoModel.GetPackageManagerExtension().PackageLoader;

            foreach (var pkg in loader.LocalPackages.ToList())
            {
                loader.Remove(pkg);
            }

            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(3, loader.LocalPackages.Count());
            Assert.IsTrue(loader.LocalPackages.Count(x => x.Name == "SignedPackage") == 1);

            currentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>() { PackagesDirectorySigned, BuiltinPackagesTestDir };

            var newPaths = new List<string> { Path.Combine(TestDirectory, "builtinpackages testdir") };
            // This function is called upon addition of new package paths in the UI.
            loader.LoadNewCustomNodesAndPackages(newPaths, currentDynamoModel.CustomNodeManager);
            Assert.AreEqual(4, loader.LocalPackages.Count());

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var builtInPkgViewModel = ViewModel.PreferencesViewModel.LocalPackages.Where(x => x.Model.BuiltInPackage).FirstOrDefault();
            Assert.IsNotNull(builtInPkgViewModel);
            Assert.AreEqual(PackageLoadState.StateTypes.Unloaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            var conflictingPkg = loader.LocalPackages.FirstOrDefault(x => x.Name == "SignedPackage" && !x.BuiltInPackage);
            Assert.IsNotNull(conflictingPkg);
            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, conflictingPkg.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, conflictingPkg.LoadState.ScheduledState);
            Assert.IsTrue(conflictingPkg.LoadedAssemblies.Count() > 0);

            ViewModel.PreferencesViewModel.InitPackageListFilters();
            var filters = ViewModel.PreferencesViewModel.Filters;
            Assert.AreEqual(3, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);
            Assert.AreEqual(@"Unloaded", filters[2].Name);

            builtInPkgViewModel.LoadCommand.Execute();

            Assert.AreEqual(PackageLoadState.StateTypes.Unloaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, conflictingPkg.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.ScheduledForDeletion, conflictingPkg.LoadState.ScheduledState);

            Assert.AreEqual(4, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);
            Assert.AreEqual(@"Scheduled for Delete", filters[2].Name);
            Assert.AreEqual(@"Unloaded", filters[3].Name);
        }

        [Test]
        [Description("User tries to unload an built-in package")]
        public void PackageManagerUninstallCommand()
        {
            var currentDynamoModel = ViewModel.Model;
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            currentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>() { BuiltinPackagesTestDir };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = currentDynamoModel.PreferenceSettings,
            };
            var loader = currentDynamoModel.GetPackageManagerExtension().PackageLoader;

            // This function is called upon addition of new package paths in the UI.
            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(2, loader.LocalPackages.Count());

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var builtInPkgViewModel = ViewModel.PreferencesViewModel.LocalPackages.Where(x => x.Model.BuiltInPackage).FirstOrDefault();
            Assert.IsNotNull(builtInPkgViewModel);
            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            ViewModel.PreferencesViewModel.InitPackageListFilters();
            var filters = ViewModel.PreferencesViewModel.Filters;
            Assert.AreEqual(2, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);

            builtInPkgViewModel.UninstallCommand.Execute();

            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.ScheduledForUnload, builtInPkgViewModel.Model.LoadState.ScheduledState);

            Assert.IsTrue(currentDynamoModel.PreferenceSettings.PackageDirectoriesToUninstall.Contains(builtInPkgViewModel.Model.RootDirectory));

            Assert.AreEqual(3, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);
            Assert.AreEqual(@"Scheduled for Unload", filters[2].Name);

            builtInPkgViewModel.UnmarkForUninstallationCommand.Execute();
            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            Assert.IsFalse(currentDynamoModel.PreferenceSettings.PackageDirectoriesToUninstall.Contains(builtInPkgViewModel.Model.RootDirectory));

            Assert.AreEqual(2, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);

        }

        [Test]
        [Description("User tries to load a manually unloaded built-in package")]
        public void PackageManagerLoadManuallyUnloadedBuiltIn()
        {
            var currentDynamoModel = ViewModel.Model;
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            currentDynamoModel.PreferenceSettings.PackageDirectoriesToUninstall.Add(Path.Combine(BuiltinPackagesTestDir, "SignedPackage2"));
            currentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>() { BuiltinPackagesTestDir };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = currentDynamoModel.PreferenceSettings,
            };

            var libraryLoader = new ExtensionLibraryLoader(currentDynamoModel);

            var loader = currentDynamoModel.GetPackageManagerExtension().PackageLoader;

            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(2, loader.LocalPackages.Count());
            Assert.IsTrue(loader.LocalPackages.Count(x => x.Name == "SignedPackage") == 1);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            var builtInPkgViewModel = ViewModel.PreferencesViewModel.LocalPackages.Where(x => x.Model.BuiltInPackage).FirstOrDefault();
            Assert.IsNotNull(builtInPkgViewModel);
            Assert.AreEqual(PackageLoadState.StateTypes.Unloaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            ViewModel.PreferencesViewModel.InitPackageListFilters();
            var filters = ViewModel.PreferencesViewModel.Filters;
            Assert.AreEqual(3, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);
            Assert.AreEqual(@"Unloaded", filters[2].Name);


            builtInPkgViewModel.LoadCommand.Execute();

            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, builtInPkgViewModel.Model.LoadState.State);
            Assert.AreEqual(PackageLoadState.ScheduledTypes.None, builtInPkgViewModel.Model.LoadState.ScheduledState);

            Assert.IsFalse(currentDynamoModel.PreferenceSettings.PackageDirectoriesToUninstall.Contains(builtInPkgViewModel.Model.RootDirectory));
            Assert.IsTrue(currentDynamoModel.SearchModel.Entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello") == 1);

            Assert.AreEqual(2, filters.Count);
            Assert.AreEqual(@"All", filters[0].Name);
            Assert.AreEqual(@"Loaded", filters[1].Name);
        }

        [Test]
        [Description("User tries to re-add a package path. Should not see any duplicate nodes")]
        public void PackageManagerNoDuplicatesOnPathIsRemovedThenAdded()
        {
            var currentDynamoModel = ViewModel.Model;

            PathManager.BuiltinPackagesDirectory = null;
            currentDynamoModel.PreferenceSettings.DisableBuiltinPackages = true;
            currentDynamoModel.PreferenceSettings.CustomPackageFolders = new List<string>() { };
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = currentDynamoModel.PreferenceSettings,
            };

            var libraryLoader = new ExtensionLibraryLoader(currentDynamoModel);

            var loader = currentDynamoModel.GetPackageManagerExtension().PackageLoader;

            loader.LoadAll(loadPackageParams);
            Assert.AreEqual(1, loader.LocalPackages.Count());
            var vm = new PackagePathViewModel(loader, loadPackageParams, Model.CustomNodeManager);

            vm.RequestShowFileDialog += (sender, args) => { args.Path = BuiltinPackagesTestDir; };
            //add a new path to SignedPackage2
            vm.AddPathCommand.Execute(null);

            //save the new path 
            vm.SaveSettingCommand.Execute(null);

            var pkg = loader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(pkg, "Expected Signed package to be valid");
            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, pkg.LoadState.State);
            Assert.AreEqual(1, currentDynamoModel.SearchModel.Entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"));

            // remove the path to SignedPackage2
            vm.DeletePathCommand.Execute(vm.RootLocations.Count - 1);

            // save
            vm.SaveSettingCommand.Execute(null);

            Assert.AreEqual(1, loader.LocalPackages.Count(x => x.Name == "SignedPackage"));
            Assert.AreEqual(1, currentDynamoModel.SearchModel.Entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"));

            // re-add the path to SignedPackage2
            vm.AddPathCommand.Execute(null);

            //save the new path 
            vm.SaveSettingCommand.Execute(null);

            Assert.AreEqual(1, loader.LocalPackages.Count(x => x.Name == "SignedPackage"));
            Assert.AreEqual(1, currentDynamoModel.SearchModel.Entries.Count(x => x.FullName == "SignedPackage2.SignedPackage2.SignedPackage2.Hello"));
        }

        public void PackageContainingNodeViewOnlyCustomization_AddsCustomizationToCustomizationLibrary()
        {
            var dynamoModel = ViewModel.Model;
            var pathManager = new Mock<Dynamo.Interfaces.IPathManager>();
            pathManager.SetupGet(x => x.PackagesDirectories).Returns(() => new[] { PackagesDirectory });
            pathManager.SetupGet(x => x.CommonDataDirectory).Returns(() => string.Empty);

            var loader = new PackageLoader(pathManager.Object);
            var libraryLoader = new ExtensionLibraryLoader(dynamoModel);

            loader.RequestLoadNodeLibrary += libraryLoader.LoadNodeLibrary;
            var loadPackageParams = new LoadPackageParams
            {
                Preferences = ViewModel.Model.PreferenceSettings

            };
            loader.LoadAll(loadPackageParams);
            //verify the UI assembly was imported from the correct package.
            var testPackage = loader.LocalPackages.FirstOrDefault(x => x.Name == "NodeViewCustomizationTestPackage");
            var uiassembly = testPackage.LoadedAssemblies.FirstOrDefault(a => a.Name == "NodeViewCustomizationAssembly");
            var nodeModelAssembly = testPackage.LoadedAssemblies.FirstOrDefault(a => a.Name == "NodeModelAssembly");
            Assert.IsNotNull(uiassembly);
            Assert.IsNotNull(nodeModelAssembly);
            //verify is marked as nodelib
            Assert.IsTrue(uiassembly.IsNodeLibrary);
            //verify that the customization was added to the customization library
            Assert.IsTrue(View.nodeViewCustomizationLibrary.ContainsCustomizationForNodeModel(nodeModelAssembly.Assembly.GetType("NodeModelAssembly.NodeModelDerivedClass")));

            loader.RequestLoadNodeLibrary -= libraryLoader.LoadNodeLibrary;
        }

        [Test]
        [Description("User tries to download packages which throws an error")]
        public void PackageManagerDownloadError()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();

            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<HeaderVersionDownload>())).Throws(new Exception());

            {
                var name = "Test";
                var version = "0.0.1";
                var id = "test-123";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = name } };
                var depVers = new List<string>() { version };

                var pkgVer = new PackageVersion()
                {
                    version = version,
                    engine_version = "2.1.1",
                    name = name,
                    id = id,
                    full_dependency_ids = deps,
                    full_dependency_versions = depVers
                };

                pmVm.ExecutePackageDownload(id, pkgVer, "");

                // Users should get 2 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with the same name and version already exists as part of the BuiltinPackages.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), string.Format(Dynamo.Wpf.Properties.Resources.MessageFailedToDownloadPackageVersion, version, id),
                            Dynamo.Wpf.Properties.Resources.PackageDownloadErrorMessageBoxTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error), Times.Exactly(1));
            }
        }

        [Test]
        [Description("User tries to download a package with dependencies on other packages and they install in correct order.")]
        public void PackageManagerDownloadsBeforeInstalling()
        {
            //keep track of download and load operations
            var operations = new List<string>();

            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();

            var mockGreg = new Mock<IGregClient>();

            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty, false);
            var pmVmMock = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };
            var sharedEngineVersion = DynamoModel.Version;


            //when we attempt a download - it returns a valid download path, also record order
            pmVmMock.Setup(x => x.Download(It.IsAny<PackageDownloadHandle>())).
                Returns<PackageDownloadHandle>(h => Task.Factory.StartNew(() =>
                {
                    //our downloads should take different amounts of time.
                    var dlTime = 0;
                    switch (h.Name)
                    {
                        case "PackageWithDep123":
                            dlTime = 10;
                            break;
                        case "Dep123":
                            dlTime = 200;
                            break;
                    }
                    Thread.Sleep(dlTime);
                    operations.Add($"download operation:{h.Name}");
                    return (h, h.Name);
                }));

            //these are our fake packages

            var dep_name = "Dep123";
            var dep_version = "0.0.1";
            var dep_id = "Dep123";
            var dep_deps = new List<Dependency>() { new Dependency() { _id = dep_id, name = dep_name } };
            var dep_depVers = new List<string>() { dep_version };

            var dep_pkgVer = new PackageVersion()
            {
                version = dep_version,
                engine_version = sharedEngineVersion,
                name = dep_name,
                id = dep_id,
                full_dependency_ids = dep_deps,
                full_dependency_versions = dep_depVers
            };

            var name = "PackageWithDep123";
            var version = "0.0.1";
            var id = "PackageWithDep123";
            var deps = new List<Dependency>() { new Dependency() { _id = id, name = name }, new Dependency() { _id = dep_id, name = dep_name } };
            var depVers = new List<string>() { version, dep_version };

            var pkgVer = new PackageVersion()
            {
                version = version,
                engine_version = sharedEngineVersion,
                name = name,
                id = id,
                full_dependency_ids = deps,
                full_dependency_versions = depVers
            };

            //when headers are retrieved for dependencies return the correct header
            clientmock.Setup(x => x.GetPackageVersionHeader(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((i, v) =>
             {
                 switch (i)
                 {
                     case "PackageWithDep123":
                         return pkgVer;

                     case "Dep123":
                         return dep_pkgVer;
                     default:
                         return null;
                 }
             });
            //record order of install.
            pmVmMock.Setup(x => x.InstallPackage(It.IsAny<PackageDownloadHandle>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<PackageDownloadHandle, string, string>((h, d, i) => { operations.Add($"install operation:{h.Name}"); });

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            //click ok during download.
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            Task.Run(() => {
                // This test runs on a single thread (using DispatcherSyncronizationContext)
                // We need to run the async ExecutePackageDownload function in a separate thread so that the Thread.Sleep does not mess up the other awaiting tasks.
                //actually perform the download & install operations
                pmVmMock.Object.ExecutePackageDownload(id, pkgVer, "");
            }).Wait();

            //wait a bit.
            Thread.Sleep(500);

            //assert that all downloads are complete before installs,
            // and install order is determined by topological order, not download completion order.
            var expectedResults = new List<string>()
            {
                "download operation:PackageWithDep123",
                "download operation:Dep123",
                "install operation:Dep123",
                "install operation:PackageWithDep123",
            };
            Assert.AreEqual(4, operations.Count);
            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.AreEqual(expectedResults[i], operations[i]);
            }
        }

        [Test]
        [Description("User tries to download a package that target a different host.")]
        public void PackageManagerWarnWhenInstallingPackageTargetingOtherHost()
        {

            var mockGreg = new Mock<IGregClient>();

            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty, false);
            var pmVmMock = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object);
            var pmMock = new Mock<PackageManagerExtension>();


            //when we attempt a download - it returns a valid download path
            pmVmMock.Setup(x => x.Download(It.IsAny<PackageDownloadHandle>())).
                Returns<PackageDownloadHandle>(h => Task.Factory.StartNew(() => (h, h.Name)));

            //these are our fake packages
            var dep_name = "Dep123";
            var dep_version = "0.0.1";
            var dep_id = "Dep123";
            var dep_deps = new List<Dependency>() { new Dependency() { _id = dep_id, name = dep_name } };
            var dep_depVers = new List<string>() { dep_version };
            var host_dependencies = new List<string>() { "Revit" };

            var dep_pkgVer = new PackageVersion()
            {
                version = dep_version,
                engine_version = "2.1.1",
                name = dep_name,
                id = dep_id,
                full_dependency_ids = dep_deps,
                full_dependency_versions = dep_depVers,
                host_dependencies = host_dependencies
            };

            var name = "PackageWithDep123";
            var version = "0.0.1";
            var id = "PackageWithDep123";
            var deps = new List<Dependency>() { new Dependency() { _id = id, name = name }, new Dependency() { _id = dep_id, name = dep_name } };
            var depVers = new List<string>() { version, dep_version };

            var pkgVer = new PackageVersion()
            {
                version = version,
                engine_version = "2.1.1",
                name = name,
                id = id,
                full_dependency_ids = deps,
                full_dependency_versions = depVers
            };

            //when headers are retrieved for dependencies return the correct header
            clientmock.Setup(x => x.GetPackageVersionHeader(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((i, v) =>
            {
                switch (i)
                {
                    case "PackageWithDep123":
                        return pkgVer;

                    case "Dep123":
                        return dep_pkgVer;
                    default:
                        return null;
                }
            });

            pmVmMock.Setup(x =>
                x.InstallPackage(It.IsAny<PackageDownloadHandle>(), It.IsAny<string>(), It.IsAny<string>()));

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();

            //click ok during download.
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            clientmock.Setup(x => x.GetKnownHosts()).Returns(new List<string>() { "Revit", "Civil 3D", "FormIt" });
            pmMock.Setup(x => x.PackageManagerClient).Returns(clientmock.Object);
            //the packageClientVM should return our mocked package manager.
            pmVmMock.Setup(x => x.PackageManagerExtension).Returns(pmMock.Object);

            // Host is Sandbox, empty string
            // This will produce a warning dialog
            pmMock.Setup(x => x.Host).Returns("");

            //actually perform the download & install operations
            pmVmMock.Object.ExecutePackageDownload(id, pkgVer, "");

            // Host is Revit
            // This will not produce a warning dialog
            pmMock.Setup(x => x.Host).Returns("Revit");

            //actually perform the download & install operations
            pmVmMock.Object.ExecutePackageDownload(id, pkgVer, "");

            // Host is Civil 3D
            // This will produce a warning dialog
            pmMock.Setup(x => x.Host).Returns("Civil 3D");

            //actually perform the download & install operations
            pmVmMock.Object.ExecutePackageDownload(id, pkgVer, "");

            // We should now have showed the warning dialog twice
            dlgMock.Verify(x => x.Show(It.IsAny<Window>(), Dynamo.Wpf.Properties.Resources.MessagePackageTargetOtherHosts,
                Dynamo.Wpf.Properties.Resources.PackageDownloadMessageBoxTitle,
                MessageBoxButton.OKCancel, MessageBoxImage.Exclamation), Times.Exactly(2));
        }

        [Test]
        [Description("User tries to download a package with dependencies on other packages but some fail to download.")]
        public void InstallsPackagesEvenIfSomeFailToDownloadShouldNotThrow()
        {
            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty, false);
            var pmVmMock = new PackageManagerClientViewModel(ViewModel, clientmock.Object);
            Assert.DoesNotThrow(() =>
            {
                pmVmMock.InstallPackage(new PackageDownloadHandle(), string.Empty, string.Empty);
                pmVmMock.InstallPackage(new PackageDownloadHandle() { DownloadState = PackageDownloadHandle.State.Error }, "somepath", "somepath");
            });


        }

        [Test]
        [Description("User tries to download packages that might conflict with an unloaded builtIn package")]
        public void PackageManagerConflictsUnloadedWithBltInPackage()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            // Load a builtIn package
            var builtInPackageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(builtInPackageLocation);

            var bltInPackage = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(bltInPackage);

            string expectedDownloadPath = "download/" + bltInPackage.ID + "/" + bltInPackage.VersionName;
            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Callback((Request x) =>
            {
                Assert.AreEqual(expectedDownloadPath, x.Path);
            });

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            //
            // 1. User downloads the exact version of a builtIn package
            //
            {
                var deps = new List<Dependency>() { new Dependency() { _id = bltInPackage.ID, name = bltInPackage.Name } };
                var depVers = new List<string>() { bltInPackage.VersionName };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = bltInPackage.VersionName,
                        engine_version = bltInPackage.EngineVersion,
                        name = bltInPackage.Name,
                        id = bltInPackage.ID,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                // Set built in package as unloaded
                bltInPackage.LoadState.SetAsUnloaded();

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(bltInPackage.Name, VersionUtilities.PartialParse(bltInPackage.VersionName));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 1 warning :
                // 1. To confirm that they want to download the specified package.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
                dlgMock.ResetCalls();
            }
        }

        [Test]
        [Description("User tries to download packages might conflict with an existing package in error state")]
        public void PackageManagerErrorStatePackages()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();

            // Load a package
            var packageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            pkgLoader.ScanPackageDirectory(packageLocation);
            var pkg = pkgLoader.LocalPackages.Where(x => x.Name == "SignedPackage").FirstOrDefault();
            Assert.IsNotNull(pkg);

            pkgLoader.LoadPackages(new List<Package>() { pkg });

            pkg.LoadState.SetAsError("Test error");

            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.Is<string>(x => x.Contains("conflicts with a different version")), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<string[]>(), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.Cancel);
            dlgMock.Setup(m => m.Show(It.Is<string>(x => x.Contains("Are you sure you want to install")), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
               .Returns(MessageBoxResult.OK);

            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            //
            // 1. User downloads a diff version of a loaded package in error state
            //
            {
                var bltinpackagesPkgVers = VersionUtilities.PartialParse(pkg.VersionName);
                var newPkgVers = new Version(bltinpackagesPkgVers.Major + 1, bltinpackagesPkgVers.Minor, bltinpackagesPkgVers.Build).ToString();

                var id = "test-123";
                var deps = new List<Dependency>() { new Dependency() { _id = id, name = pkg.Name } };
                var depVers = new List<string>() { newPkgVers };

                mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = newPkgVers,
                        engine_version = pkg.EngineVersion,
                        name = pkg.Name,
                        id = id,
                        full_dependency_ids = deps,
                        full_dependency_versions = depVers
                    },
                    success = true
                });

                var pkgInfo = new Dynamo.Graph.Workspaces.PackageInfo(pkg.Name, VersionUtilities.PartialParse(pkg.VersionName));
                pmVm.DownloadAndInstallPackage(pkgInfo);

                // Users should get 1 warnings :
                // 1. To confirm that they want to download the specified package.
                // 2. That a package with the same name and version already exists.
                dlgMock.Verify(x => x.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Exactly(1));
                dlgMock.ResetCalls();
            }
        }

        [Test]
        [Description("User tries to download package in Dynamo 3.x which was published in Dynamo 2.x")]
        public void PackageManagerShowsWarningWhenDownloadingLegacyPackage()
        {
            var pathMgr = ViewModel.Model.PathManager;
            var pkgLoader = GetPackageLoader();

            // Simulate the user downloading the same package from PM
            var mockGreg = new Mock<IGregClient>();
            var client = new Dynamo.PackageManager.PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmVm = new PackageManagerClientViewModel(ViewModel, client);

            var pkgVer = new Version(1, 0, 0).ToString();
            var pkgEngineVersion = new Version(2, 0, 0).ToString();
            var id = "test-123";
            var name = "test-123-name";
            var deps = new List<Dependency>() { new Dependency() { _id = id, name = name } };
            var depVers = new List<string>() { pkgVer };

            var pkgVersionObject = new PackageVersion()
            {
                version = pkgVer,
                engine_version = pkgEngineVersion,
                name = name,
                id = id,
                full_dependency_ids = deps,
                full_dependency_versions = depVers
            };

            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = pkgVersionObject,
                    success = true
                });

            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);
            pmVm.ExecutePackageDownload(id, pkgVersionObject, "");

            // Users should get 2 warnings :
            // 1. To confirm that they want to download the specified package.
            // 2. That this package is a legacy package.
            dlgMock.Verify(x => x.Show(null,
                       $"{string.Format(Dynamo.Wpf.Properties.Resources.MessagePackageOlderDynamo, ViewModel.BrandingResourceProvider.ProductName)} {Dynamo.Wpf.Properties.Resources.MessagePackOlderDynamoLink}",
                       string.Format(Dynamo.Wpf.Properties.Resources.PackageUseOlderDynamoMessageBoxTitle, ViewModel.BrandingResourceProvider.ProductName),
                       true,
                        MessageBoxButton.OKCancel,MessageBoxImage.Exclamation), Times.Exactly(1));

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

        /// <summary>
        ///     Checks the functionality of the InitialResultsLoaded property
        /// </summary>
        [Test]
        public void PackageInitialResultsLoaded()
        {
            // Arrange
            PackageManagerSearchViewModel searchViewModel = new PackageManagerSearchViewModel();

            // Assert
            Assert.AreEqual(false, searchViewModel.InitialResultsLoaded);

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Results;

            // Assert
            Assert.AreEqual(true, searchViewModel.InitialResultsLoaded);

            // Act
            searchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Searching;

            // Assert
            Assert.AreEqual(true, searchViewModel.InitialResultsLoaded);
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
                Console.WriteLine("Failed to load the package: " + e);
            }

            var loader = GetPackageLoader();
            var packageFound = loader.LocalPackages.Any(x => x.Name == "Autodesk Steel Connections 2020");
            Assert.IsFalse(packageFound);

            var preferencesWindow = new PreferencesView(View)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            preferencesWindow.Show();

            AssertWindowOwnedByDynamoView<PreferencesView>();
        }



        #endregion

        #region PackageManagerView

        [Test]
        public void CanOpenPackageManagerAndWindowIsOwned()
        {
            ViewModel.OnRequestPackageManagerDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void CannotCreateDuplicatePackageManagerDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                ViewModel.OnRequestPackageManagerDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void PackageManagerClosesWithDynamo()
        {
            ViewModel.OnRequestPackageManagerDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerView>();
            AssertWindowClosedWithDynamoView<PackageManagerView>();
        }

        [Test]
        public void SearchBoxInactiveOnWindowOpened()
        {
            ViewModel.OnRequestPackageManagerDialog(null, null);

            var windows = GetWindowEnumerable(View.OwnedWindows);
            var packageManagerView = windows.First(x => x is PackageManagerView) as PackageManagerView;

            Assert.IsNotNull(packageManagerView);

            var searchBox = LogicalTreeHelper.FindLogicalNode(packageManagerView, "SearchBox") as UserControl;
            Assert.IsNotNull(searchBox);
            Assert.IsFalse(searchBox.IsEnabled);

            packageManagerView.PackageManagerViewModel.PackageSearchViewModel.InitialResultsLoaded = true;
            Assert.IsTrue(searchBox.IsEnabled);
        }

        [Test]
        public void PackageManagerDialogDoesNotThrowExceptions()
        {
            Assert.DoesNotThrow(() => ViewModel.OnRequestPackageManagerDialog(null, null), "Package Manager View did not open without exceptions");

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }


        /// <summary>
        ///     Asserts that the filter context menu will stay open while the user interacts with it
        /// </summary>
        [Test]
        public void FiltersContextMenuStaysOpen()
        {
            ViewModel.OnRequestPackageManagerDialog(null, null);
            Thread.Sleep(500);

            var windows = GetWindowEnumerable(View.OwnedWindows);
            var pacakgeManager = windows.First(x => x is PackageManagerView) as PackageManagerView;
            var pacakgeManagerSearchControl = pacakgeManager.packageManagerSearch;
            Assert.IsNotNull(pacakgeManagerSearchControl);

            var button = pacakgeManager.packageManagerSearch.filterResultsButton;
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            var hostFilterContextMenu = pacakgeManagerSearchControl.HostFilter;
            Assert.IsTrue(hostFilterContextMenu.IsOpen);

            var menuItemOne = hostFilterContextMenu.Items[1] as PackageManagerSearchViewModel.FilterEntry;
            Assert.IsNotNull(menuItemOne);

            menuItemOne.FilterCommand.Execute(null);
            Assert.IsTrue(hostFilterContextMenu.IsOpen);
        }

        #endregion

        #region PackageViewModel Interaction

        [Test]
        public void AddsFilesAndFoldersFromFilePathsCorrectly()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\PackageWithNodeDocumentation");
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
            Assert.AreEqual(1, packageContents.Count); // We expect only 1 root item here

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
            //since we donot skip loading asemblies which are already loaded..
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

            var additionalFiles = vm.AdditionalFiles;

            var packageContents = vm.PackageContents;
            Assert.AreEqual(packageContents.Count, 2); // We expect 2 separate root item here

            // Assert
            // that the PackageContents contains the correct number of items
            var allFilesAndFoldres = PackageItemRootViewModel.GetFiles(packageContents.ToList());

            // add 2 root folders
            Assert.AreEqual(allFilesAndFoldres.Count, allFiles.Count + allFolders.Count + (allDuplicateFiles.Count) + (allDuplicateFolders.Count) + 2);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);

            // add 2 root folders
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), allFolders.Count + (allDuplicateFolders.Count) + 2);

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

            // add 1 root folder
            Assert.AreEqual(allFilesAndFoldres.Count, (commonRootFiles.Count) + (commonRootFolders.Count) + 1);
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.File)), additionalFiles.Count);

            // add 1 root folder1
            Assert.AreEqual(allFilesAndFoldres.Count(i => i.DependencyType.Equals(DependencyType.Folder)), (commonRootFolders.Count) + 1);
        }


        [Test]
        public void GetAllFilesReturnsEqualsPackageContents()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            vm.AddAllFilesAfterSelection(allFiles);

            int packageContentsCount = 0;

            foreach (var rootItem in vm.PackageContents)
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

            vm.AddAllFilesAfterSelection(allFiles);

            var testPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\", "TestPath");
            var testPkgName = @"Test Package";

            var rootItemPreview = vm.GetPreBuildRootItemViewModel(testPath, testPkgName, allFiles);
            var allRootItems = PackageItemRootViewModel.GetFiles(rootItemPreview);

            var folders = allRootItems.Count(x => x.DependencyType.Equals(DependencyType.Folder));
            var files = allRootItems.Count(x => !x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(5, folders);
            Assert.AreEqual(4, files);
        }


        [Test]
        public void AssertGetExistingRootItemViewModelReturnsCorrectItem()
        {
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

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

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            Assert.AreEqual(1, vm.PackageContents.Count);
            Assert.AreEqual(1, vm.PreviewPackageContents.Count);
            var childItems = vm.PackageContents.First().ChildItems;

            // Assert
            foreach (var child in childItems)
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
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\_AllFileTypesPackageDocs");
            string dyfPath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\_AllFileTypesPackageDocs\\dyf\\3DView by BoundingBox.dyf");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

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
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\_AllFileTypesPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            vm.AddAllFilesAfterSelection(allFiles);

            // Act
            var rootFolder = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(1, rootFolder.Count());

            var childItems = PackageItemRootViewModel.GetFiles(rootFolder.First());

            var files = childItems.Where(x => x.DependencyType.Equals(DependencyType.File));
            var dyfPreviewFiles = childItems.Where(x => x.DependencyType.Equals(DependencyType.CustomNodePreview));
            var folders = childItems.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            Assert.AreEqual(4, files.Count());
            Assert.AreEqual(1, dyfPreviewFiles.Count());
            Assert.AreEqual(4, folders.Count());

            // Assert
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(dyfPreviewFiles.First()));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(files.First(x => x.DisplayName.EndsWith(".json"))));
            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(files.First(x => x.DisplayName.EndsWith(".xml"))));

            // At this point, only one root item remains, not the original one but the 'doc' folder
            // The original root item no longer contains a file, therefore it was removed
            // This makes sense as we don't want to try to establish 'common parent' for folders that maybe too far apart in a tree structure
            rootFolder = vm.PackageContents.Where(x => x.DependencyType.Equals(DependencyType.Folder));
            Assert.AreEqual(1, rootFolder.Count());
            Assert.AreEqual(3, PackageItemRootViewModel.GetFiles(rootFolder.First()).Count());  // the 'doc' folder + the 2 files inside of it

            Assert.DoesNotThrow(() => vm.RemoveItemCommand.Execute(rootFolder.First()));
            Assert.IsFalse(vm.PackageContents.Any());
        }

        // This test asserts CancelCommand, which is currently disabled under testing environment
        // as it is causing a tread affinity crash. The test will be disabled for the time being
        [Test, Category("Failure")]
        public void CancelCommandClearsAllData()
        {
            // Arrange
            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\RootPackageFolder\\PackageWithNodeDocumentation");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();
            var vm = new PublishPackageViewModel(this.ViewModel);

            Assert.AreEqual(0, vm.PackageContents.Count);
            Assert.AreEqual(0, vm.PreviewPackageContents.Count);

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
            var publishPath = Path.Combine(Path.GetTempPath(), packageName);

            var nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            // now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);

            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            // Arrange - mockup the whole logic
            var pkg = new Package(publishPath, packageName, "1.0.0", "MIT");    // The Package 
            var files = newPkgVm.BuildPackage();    // We get the files as we would do in the process
            var pkgsDir = Path.GetTempPath();   // The folder to install the package into

            var fs = new RecordedFileSystem((fn) => files.Contains(fn));
            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            // Act
            db.BuildDirectory(pkg, pkgsDir, files, Enumerable.Empty<string>());

            // Assert
            Assert.AreEqual(fs.CopiedFiles.Count(), previewFiles.Count() - 1); // The original pkg.json will be skipped
            Assert.AreEqual(fs.DirectoriesCreated.Count(), previewFolders.Count()); 
        }

        [Test]
        public void AssertPreviewPackageRetainFolderStructureEqualsPublishLocalPackageResults()
        {
            var packageName = "SingleFolderPublishPackage";
            var publishPath = Path.Combine(Path.GetTempPath(), packageName);

            var nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            // now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);
            newPkgVm.RetainFolderStructureOverride = true;
            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            // Arrange - mockup the whole logic
            var pkg = new Package(publishPath, packageName, "1.0.0", "MIT");    // The Package 
            var files = newPkgVm.BuildPackage();    // We get the files as we would do in the process
            var updatedFiles = newPkgVm.UpdateFilesForRetainFolderStructure(files); // We update the files as we would do in the process
            var pkgsDir = Path.GetTempPath();   // The folder to install the package into
            var roots = newPkgVm.GetCommonPaths(files.ToArray());   // We use the same function the process is using to create the roots

            var fs = new RecordedFileSystem((fn) => updatedFiles.SelectMany(files => files).ToList().Any((x) => ComparePaths(x, fn)));
            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            // Act
            db.BuildRetainDirectory(pkg, pkgsDir, roots, updatedFiles, Enumerable.Empty<string>());

            Assert.IsTrue(Directory.Exists(publishPath));

            var createdFolders = Directory.GetDirectories(publishPath, "*", SearchOption.AllDirectories).ToList();

            // Assert
            Assert.AreEqual(fs.CopiedFiles.Count(), previewFiles.Count());
            Assert.AreEqual(0, createdFolders.Count());

            // Clean up
            Directory.Delete(publishPath, true);
        }

        [Test]
        public void AssertPreviewPackageRetainFolderStructureEqualsPublishLocalPackageResultsForNestedFolders()
        {
            var packageName = "NestedPackage";
            var publishPath = Path.Combine(Path.GetTempPath(), packageName);
            var nodePath = Path.Combine(TestDirectory, "pkgs", packageName);
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            // now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);
            newPkgVm.RetainFolderStructureOverride = true;
            newPkgVm.AddAllFilesAfterSelection(allFiles);

            var previewFilesAndFolders = PackageItemRootViewModel.GetFiles(newPkgVm.PreviewPackageContents.ToList());
            var previewFiles = previewFilesAndFolders.Where(x => !x.DependencyType.Equals(DependencyType.Folder));
            var previewFolders = previewFilesAndFolders.Where(x => x.DependencyType.Equals(DependencyType.Folder));

            // Arrange - mockup the whole logic
            var pkg = new Package(publishPath, packageName, "1.0.0", "MIT");    // The Package 
            var files = newPkgVm.BuildPackage();    // We get the files as we would do in the process
            var updatedFiles = newPkgVm.UpdateFilesForRetainFolderStructure(files); // We update the files as we would do in the process
            var pkgsDir = Path.GetTempPath();   // The folder to install the package into
            var roots = newPkgVm.GetCommonPaths(files.ToArray());   // We use the same function the process is using to create the roots

            var fs = new RecordedFileSystem((fn) => updatedFiles.SelectMany(files => files).ToList().Any((x) => ComparePaths(x, fn)));
            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            // Act
            db.BuildRetainDirectory(pkg, pkgsDir, roots, updatedFiles, Enumerable.Empty<string>());

            Assert.IsTrue(Directory.Exists(publishPath));

            var createdFolders = Directory.GetDirectories(publishPath, "*", SearchOption.AllDirectories).ToList();

            // Assert
            Assert.AreEqual(fs.CopiedFiles.Count(), previewFiles.Count());
            Assert.AreEqual(3, createdFolders.Count());

            // Clean up
            Directory.Delete(publishPath, true);
        }

        [Test]
        [Ignore("This is testing a very obvious side effect of the publish routine, it's not worth installing a package for it. + no clean up.")]
        public void AssertPublishLocalHandleType()
        {
            var packageName = "SingleFolderPublishPackage";

            string nodePath = Path.Combine(TestDirectory, "core", "docbrowser\\pkgs\\SingleFolderPublishPackageDocs");
            var allFiles = Directory.GetFiles(nodePath, "*", SearchOption.AllDirectories).ToList();

            //now lets publish this package.
            var newPkgVm = new PublishPackageViewModel(this.ViewModel);
            newPkgVm.RetainFolderStructureOverride = true;
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
        }

        [Test]
        public void AssertPublishNewPackageVersion_SuccessfulForLoadedPackages()
        {
            var packageName = "Package";
            var pkgLoader = GetPackageLoader();
            var publishPath = Path.Combine(Path.GetTempPath(), packageName);
            Directory.CreateDirectory(publishPath);

            // Load a package
            string packageLocation = Path.Combine(GetTestDirectory(ExecutingDirectory), "pkgs", packageName);
            pkgLoader.ScanPackageDirectory(packageLocation);

            var package = pkgLoader.LocalPackages.Where(x => x.Name == packageName).FirstOrDefault();
            package.SetAsLoaded();
            Assert.IsNotNull(package);

            PublishPackageViewModel vm = null;
            Assert.DoesNotThrow(() =>
            {
                vm = PublishPackageViewModel.FromLocalPackage(ViewModel, package, true);
            });
            vm.MajorVersion = "0";
            vm.MinorVersion = "0";
            vm.BuildVersion = "99";

            var allFiles = vm.BuildPackage();
            Assert.IsNotNull(allFiles);

            var updatedFiles = vm.UpdateFilesForRetainFolderStructure(allFiles);
            Assert.IsNotNull(updatedFiles);

            var roots = vm.GetCommonPaths(allFiles.ToArray());
            Assert.IsNotNull(roots);


            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(package));
            var fs = new RecordedFileSystem((fn) => updatedFiles.SelectMany(files => files).ToList().Any((x) => ComparePaths(x, fn)));
            var db = new PackageDirectoryBuilder(fs, MockMaker.Empty<IPathRemapper>());

            var zipper = new Mock<IFileCompressor>();
            zipper.Setup((x) => x.Zip(It.IsAny<IDirectoryInfo>())).Returns((new Mock<IFileInfo>()).Object);
            var m = new PackageUploadBuilder(db, zipper.Object);

            // Assert
            Assert.DoesNotThrow(() =>
            {
                m.NewPackageVersionRetainUpload(package, publishPath, roots, updatedFiles, Enumerable.Empty<string>(), handle);
            });
            Assert.AreNotEqual(PackageUploadHandle.State.Error, handle.UploadState);

            // Clean up
            Directory.Delete(publishPath, true);
        }
        #endregion
    }
}
