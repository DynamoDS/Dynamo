using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

using SystemTestServices;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.ViewModels;
using NUnit.Framework;


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
            var vm = new PackagePathViewModel(setting);

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

            var vm = new PackagePathViewModel(setting);

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

            var vm = new PackagePathViewModel(setting);

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
        #endregion
    }
}
