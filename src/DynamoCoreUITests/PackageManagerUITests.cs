using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class PackageManagerUITests
    {
        private static DynamoController controller;
        private static DynamoViewModel vm;
        private static DynamoView ui;

        #region SetUp & TearDown

        [SetUp, RequiresSTA]
        public void Start()
        {
            controller = DynamoController.MakeSandbox();

            //create the view
            ui = new DynamoView();
            ui.DataContext = controller.DynamoViewModel;
            vm = controller.DynamoViewModel;
            controller.UIDispatcher = ui.Dispatcher;
            ui.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TearDown, RequiresSTA]
        public void Exit()
        {
            if (ui.IsLoaded)
                ui.Close();
        }

        #endregion

        #region Utility functions

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
            var windows = GetWindowEnumerable(ui.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)ui);
        }

        public void AssertWindowClosedWithDynamoView<T>()
        {
            var windows = GetWindowEnumerable(ui.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)ui);
        }

        #endregion

        #region PackageManagerPublishView

        [Test, RequiresSTA]
        public void CanOpenPackagePublishDialogAndWindowIsOwned()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test, RequiresSTA]
        public void CannotCreateDuplicatePackagePublishDialogs()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestPackagePublishDialog(l);
            }

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test, RequiresSTA]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
            AssertWindowClosedWithDynamoView<PackageManagerPublishView>();

        }

        #endregion

        #region InstalledPackagesView

        [Test, RequiresSTA]
        public void CanOpenManagePackagesDialogAndWindowIsOwned()
        {
            vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        [Test, RequiresSTA]
        public void CannotCreateDuplicateManagePackagesDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestManagePackagesDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        [Test, RequiresSTA]
        public void ManagePackagesDialogClosesWithDynamo()
        {
            vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
            AssertWindowClosedWithDynamoView<InstalledPackagesView>();

        }

        #endregion

        #region PackageManagerSearchView

        [Test, RequiresSTA]
        public void CanOpenPackageSearchDialogAndWindowIsOwned()
        {
            vm.OnRequestPackageManagerSearchDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test, RequiresSTA]
        public void CannotCreateDuplicatePackageSearchDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestPackageManagerSearchDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test, RequiresSTA]
        public void PackageSearchDialogClosesWithDynamo()
        {
            vm.OnRequestPackageManagerSearchDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
            AssertWindowClosedWithDynamoView<PackageManagerSearchView>();

        }

        #endregion

    }
}
