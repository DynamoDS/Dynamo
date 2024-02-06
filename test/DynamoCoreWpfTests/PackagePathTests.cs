
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using SystemTestServices;
using Moq;
using System.Collections.Generic;
using Dynamo.Extensions;
using System;

namespace DynamoCoreWpfTests
{
    public class PackagePathTests : SystemTestBase
    {
        private static string executingDirectory;
        protected static string ExecutingDirectory
        {
            get
            {
                if (executingDirectory == null)
                {
                    executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return executingDirectory;
            }
        }

        private static string testDirectory;
        internal static string TestDirectory
        {
            get
            {
                if (testDirectory == null)
                {
                    var directory = new DirectoryInfo(ExecutingDirectory);
                    testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
                }
                return testDirectory;
            }
        }
        internal string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }

        internal string BuiltInPackagesTestDir { get { return Path.Combine(TestDirectory, "builtinpackages testdir", "Packages"); } }

        #region PackagePathViewModelTests
        [Test]
        public void CannotDeletePathIfThereIsOnlyOne()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(1, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(null));
        }

        [Test]
        public void ReorderingPathsTest()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"C:\", @"D:\", @"E:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(0));
            Assert.IsFalse(vm.MovePathUpCommand.CanExecute(0));

            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(2));
            Assert.IsFalse(vm.MovePathDownCommand.CanExecute(2));

            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(1));
            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(1));

            vm.MovePathUpCommand.Execute(1);

            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"E:\", vm.RootLocations[2]);

            vm.MovePathDownCommand.Execute(1);

            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"E:\", vm.RootLocations[1]);
            Assert.AreEqual(@"C:\", vm.RootLocations[2]);
        }

        [Test]
        public void CannotDeleteBuiltinPackagesPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { DynamoModel.BuiltInPackagesToken, @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(0));
            Assert.IsTrue(vm.DeletePathCommand.CanExecute(1));
        }

        [Test]
        public void CannotUpdateBuiltinPackagesPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { DynamoModel.BuiltInPackagesToken, @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.UpdatePathCommand.CanExecute(0));
            Assert.IsTrue(vm.UpdatePathCommand.CanExecute(1));
        }
        [Test]
        public void CannotDeleteProgramDataPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { Path.Combine(ViewModel.Model.PathManager.CommonDataDirectory,"Packages"), @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(0));
            Assert.IsTrue(vm.DeletePathCommand.CanExecute(1));
        }

        [Test]
        public void CannotUpdateProgramDataPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { Path.Combine(ViewModel.Model.PathManager.CommonDataDirectory, "Packages"), @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.UpdatePathCommand.CanExecute(0));
            Assert.IsTrue(vm.UpdatePathCommand.CanExecute(1));
        }

        [Test]
        public void AddRemovePathsTest()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = @"C:\";
            vm.AddPathCommand.Execute(null);
            path = @"D:\";
            vm.AddPathCommand.Execute(null);

            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"D:\", vm.RootLocations[2]);

            vm.DeletePathCommand.Execute(0);

            Assert.AreEqual(@"C:\", vm.RootLocations[0]);
            Assert.AreEqual(@"D:\", vm.RootLocations[1]);
        }
        [Test]
        public void AddPackagePathsTest()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            var testDir = GetTestDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            path = Path.Combine(testDir, @"core\packagePathTest");
            var dynFilePath = Path.Combine(path, @"dynFile\Number1.dyn");
            vm.AddPathCommand.Execute(null);
            vm.SaveSettingCommand.Execute(null);
            Model.ExecuteCommand(new DynamoModel.OpenFileCommand(dynFilePath));
            Assert.AreEqual(1,GetPreviewValue("07d62dd8-b2f3-40a8-a761-013d93300444"));
        }

        [Test]
        public void InstalledPackagesContainsCorrectNumberOfPackages()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { PackagesDirectory }
            };
            var vm = CreatePackagePathViewModel(setting);
            var libraryLoader = new ExtensionLibraryLoader(ViewModel.Model);

            vm.packageLoader.PackagesLoaded += libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            var packagesLoaded = false;

            Action<IEnumerable<Assembly>> pkgsLoadedDelegate = (x) => { packagesLoaded = true; };
            vm.packageLoader.PackagesLoaded += pkgsLoadedDelegate;

            vm.packageLoader.LoadAll(vm.loadPackageParams);
            Assert.AreEqual(20, vm.packageLoader.LocalPackages.Count());
            Assert.AreEqual(true, packagesLoaded);

            var installedPackagesViewModel = new InstalledPackagesViewModel(ViewModel, vm.packageLoader);
            Assert.AreEqual(20, installedPackagesViewModel.LocalPackages.Count);

            var installedPackagesView = new Dynamo.Wpf.Controls.InstalledPackagesControl();
            installedPackagesView.DataContext = installedPackagesViewModel;
            DispatcherUtil.DoEvents();

            Assert.AreEqual(20, installedPackagesView.SearchResultsListBox.Items.Count);
            Assert.AreEqual(2, installedPackagesView.Filters.Items.Count);

            vm.packageLoader.PackagesLoaded -= libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void RemoveAddPackagePathChangesInstalledPackageState()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { PackagesDirectory }
            };

            var vm = CreatePackagePathViewModel(setting);
            var libraryLoader = new ExtensionLibraryLoader(ViewModel.Model);

            vm.packageLoader.PackagesLoaded += libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            // Load packages in package path.
            vm.packageLoader.LoadAll(vm.loadPackageParams);

            Assert.AreEqual(20, vm.packageLoader.LocalPackages.Count());
            // Remove package path.
            vm.DeletePathCommand.Execute(0);

            foreach(var pkg in vm.packageLoader.LocalPackages)
            {
                Assert.True(pkg.LoadState.State == PackageLoadState.StateTypes.Loaded);
                Assert.True(pkg.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload);
            }

            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };
            path = Path.Combine(GetTestDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), "pkgs");

            //Add the path back.
            vm.AddPathCommand.Execute(null);

            foreach (var pkg in vm.packageLoader.LocalPackages)
            {
                Assert.True(pkg.LoadState.State == PackageLoadState.StateTypes.Loaded);
                Assert.True(pkg.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.None);
            }

            vm.packageLoader.PackagesLoaded -= libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void EnableCustomPackagePathsLoadsPackagesOnClosingPreferences()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { PackagesDirectory }
            };

            var vm = CreatePackagePathViewModel(setting);
            var libraryLoader = new ExtensionLibraryLoader(ViewModel.Model);

            vm.packageLoader.PackagesLoaded += libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary += libraryLoader.LoadLibraryAndSuppressZTSearchImport;

            (setting as IDisablePackageLoadingPreferences).DisableCustomPackageLocations = true;

            // Load packages in package path.
            vm.packageLoader.LoadAll(vm.loadPackageParams);

            Assert.AreEqual(0, vm.packageLoader.LocalPackages.Count());

            // simulate turning off "disable custom package paths" toggle.
            (setting as IDisablePackageLoadingPreferences).DisableCustomPackageLocations = false;
            vm.SetPackagesScheduledState(setting.CustomPackageFolders.First(), false);

            // simulate closing preferences dialog by saving changes to packagepathviewmodel 
            vm.SaveSettingCommand.Execute(null);

            // packages are expected to load from 'PackagesDirectory' above when toggle is turned off
            Assert.AreEqual(20, vm.packageLoader.LocalPackages.Count());

            vm.packageLoader.PackagesLoaded -= libraryLoader.LoadPackages;
            vm.packageLoader.RequestLoadNodeLibrary -= libraryLoader.LoadLibraryAndSuppressZTSearchImport;
        }

        [Test]
        public void PathEnabledConverterCustomPaths()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);
            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = @"C:\";
            vm.AddPathCommand.Execute(null);
            var x = new PathEnabledConverter();
            Assert.False((bool)x.Convert(new object[] { vm, path },null,null,null ));

            setting.DisableCustomPackageLocations = true;

            Assert.True((bool)x.Convert(new object[] { vm, path }, null, null, null));
        }

        [Test]
        public void PathEnabledConverterBltinpackagesPath()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);
            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = @"Dynamo Built-In Packages";
            vm.AddPathCommand.Execute(null);
            var x = new PathEnabledConverter();
            Assert.False((bool)x.Convert(new object[] { vm, path }, null, null, null));

            setting.DisableBuiltinPackages = true;

            Assert.True((bool)x.Convert(new object[] { vm, path }, null, null, null));
            Assert.False((bool)x.Convert(new object[] { vm, @"Z:\" }, null, null, null));
        }

        [Test]
        public void IfPathsAreUnchangedPackagesAreNotReloaded()
        {
            var count = 0;
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = {@"Z:\" }
            };
            var vm = CreatePackagePathViewModel(setting);
            vm.packageLoader.PackagesLoaded += Loader_PackagesLoaded;

            LoadPackageParams loadParams = new LoadPackageParams
            {
                Preferences = setting,
            };

            vm.SaveSettingCommand.Execute(null);

            //should not have reloaded anything.
            Assert.AreEqual(0, count);
            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };
            path = Path.Combine(GetTestDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), "pkgs");
            //add the new path
            vm.AddPathCommand.Execute(null);

            //save the new path 
            vm.SaveSettingCommand.Execute(null);

            //should have loaded something.
            Assert.AreEqual(10, count);

            //commit the paths again. 
            vm.SaveSettingCommand.Execute(null);

            //should not have loaded anything.
            Assert.AreEqual(10, count);

            void Loader_PackagesLoaded(System.Collections.Generic.IEnumerable<Assembly> obj)
            {
                count = count + obj.Count();
            }
            vm.packageLoader.PackagesLoaded -= Loader_PackagesLoaded;
        }

        [Test]
        public void PathsAddedToCustomPacakgePathPreferences_SurvivePreferenceDialogOpenClose()
        {
            //add a new path to the package paths
            Model.PreferenceSettings.CustomPackageFolders.Add(@"C:\doesNotExist");
            Model.PreferenceSettings.CustomPackageFolders.Add(@"C:\doesNotExist\dde.dll");

            //assert preference settings is correct after prefs window open and close.
            var preferencesWindow = new PreferencesView(View);
            //we use show because showDialog will block the test.
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();
            preferencesWindow.CloseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //assert preference settings is correct.
            Assert.Contains(@"C:\doesNotExist", Model.PreferenceSettings.CustomPackageFolders);
            Assert.Contains(@"C:\doesNotExist\dde.dll", Model.PreferenceSettings.CustomPackageFolders);
        }

        #endregion
        #region Setup methods
        private PackagePathViewModel CreatePackagePathViewModel(PreferenceSettings settings)
        {
            var pathManager = new PathManager(new PathManagerParams { })
            {
                Preferences = settings
            };

            PackageLoader loader = new PackageLoader(pathManager);
            
            LoadPackageParams loadParams = new LoadPackageParams
            {
                Preferences = settings,
            };
            CustomNodeManager customNodeManager = Model.CustomNodeManager;
            return new PackagePathViewModel(loader, loadParams, customNodeManager);
        }
        #endregion
    }

    class PackagePathTests_CustomPrefs : DynamoTestUIBase
    {
        /// <summary>
        /// Derived test classes can override this method to provide different configurations.
        /// </summary>
        /// <param name="pathResolver">A path resolver to pass to the DynamoModel. </param>
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings()
                {
                    //program data first
                    CustomPackageFolders = { Path.Combine(GetCommonDataDirectory(),PathManager.PackagesDirectoryName),  @"C:\", GetAppDataFolder(), }
                }
            };
        }

        [Test]
        public void IfProgramDataPathIsFirstDefaultPackagePathIsStillAppData()
        {
            var setting = Model.PreferenceSettings;

            var appDataFolder = GetAppDataFolder();
            Assert.AreEqual(4, ViewModel.Model.PathManager.PackagesDirectories.Count());
            Assert.AreEqual(4, setting.CustomPackageFolders.Count);
            var appDataPackagesDir = Path.Combine(appDataFolder, PathManager.PackagesDirectoryName);
            Assert.AreEqual(appDataPackagesDir, ViewModel.Model.PathManager.DefaultPackagesDirectory);
            Assert.AreEqual(appDataFolder, setting.SelectedPackagePathForInstall);
        }
    }
}
