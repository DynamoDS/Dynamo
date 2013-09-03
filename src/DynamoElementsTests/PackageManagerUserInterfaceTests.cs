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
    class PackageManagerUserInterfaceTests
    {
        private static DynamoController controller;
        private static DynamoViewModel vm;
        private static DynamoView ui;

        #region SetUp & TearDown

        [SetUp]
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

        [TearDown]
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

            while ( enumerator.MoveNext() )
            {
                yield return (Window) enumerator.Current;
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

        [Test]
        public void CanOpenPackagePublishDialogAndWindowIsOwned()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test]
        public void CannotCreateDuplicatePackagePublishDialogs()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestPackagePublishDialog(l);
            }

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
            AssertWindowClosedWithDynamoView<PackageManagerPublishView>();

        }

        #endregion

        #region InstalledPackagesView 

        [Test]
        public void CanOpenManagePackagesDialogAndWindowIsOwned()
        {
           vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        [Test]
        public void CannotCreateDuplicateManagePackagesDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestManagePackagesDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
        }

        [Test]
        public void ManagePackagesDialogClosesWithDynamo()
        {
            vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
            AssertWindowClosedWithDynamoView<InstalledPackagesView>();

        }

         #endregion

        #region PackageManagerSearchView

        [Test]
        public void CanOpenPackageSearchDialogAndWindowIsOwned()
        {
            vm.OnRequestPackageManagerSearchDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void CannotCreateDuplicatePackageSearchDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                vm.OnRequestPackageManagerSearchDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void PackageSearchDialogClosesWithDynamo()
        {
            vm.OnRequestPackageManagerSearchDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
            AssertWindowClosedWithDynamoView<PackageManagerSearchView>();

        }

         #endregion


    }
}
