
using System.IO;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    public class PackagePathTests : SystemTestBase
    {
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

            Assert.AreEqual(0, vm.SelectedIndex);
            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(null));
            Assert.IsFalse(vm.MovePathUpCommand.CanExecute(null));

            vm.SelectedIndex = 2;
            Assert.AreEqual(2, vm.SelectedIndex);
            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(null));
            Assert.IsFalse(vm.MovePathDownCommand.CanExecute(null));

            vm.SelectedIndex = 1;
            Assert.AreEqual(1, vm.SelectedIndex);
            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(null));
            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(null));

            vm.MovePathUpCommand.Execute(vm.SelectedIndex);

            Assert.AreEqual(0, vm.SelectedIndex);
            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"E:\", vm.RootLocations[2]);

            vm.SelectedIndex = 1;
            vm.MovePathDownCommand.Execute(vm.SelectedIndex);

            Assert.AreEqual(2, vm.SelectedIndex);
            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"E:\", vm.RootLocations[1]);
            Assert.AreEqual(@"C:\", vm.RootLocations[2]);
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

            Assert.AreEqual(0, vm.SelectedIndex);
            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"D:\", vm.RootLocations[2]);

            vm.SelectedIndex = 2;
            vm.DeletePathCommand.Execute(0);

            Assert.AreEqual(1, vm.SelectedIndex);
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

        #endregion
        #region Setup methods
        private PackagePathViewModel CreatePackagePathViewModel(PreferenceSettings setting)
        {
            PackageLoader loader = new PackageLoader(setting.CustomPackageFolders);
            LoadPackageParams loadParams = new LoadPackageParams
            {
                Preferences = setting,
                PathManager = Model.PathManager
            };
            CustomNodeManager customNodeManager = Model.CustomNodeManager;
            return new PackagePathViewModel(loader, loadParams, customNodeManager);
        }

        #endregion
    }
}
