using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class PackageManagerUITests : DynamoTestUI
    {
        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            Controller = DynamoController.MakeSandbox();
            Controller.Testing = true;

            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder();
            }
        }

        [TearDown]
        public void Exit()
        {
            if (Ui.IsLoaded)
                Ui.Close();
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

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
            var windows = GetWindowEnumerable(Ui.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)Ui);
        }

        public void AssertWindowClosedWithDynamoView<T>()
        {
            var windows = GetWindowEnumerable(Ui.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)Ui);
        }

        #endregion

        #region PackageManagerPublishView

        [Test]
        public void CanOpenPackagePublishDialogAndWindowIsOwned()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            Vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test,Ignore]
        public void CannotCreateDuplicatePackagePublishDialogs()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            for (var i = 0; i < 10; i++)
            {
                Vm.OnRequestPackagePublishDialog(l);
            }

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
        }

        [Test]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            Vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
            AssertWindowClosedWithDynamoView<PackageManagerPublishView>();

        }

        #endregion

        #region InstalledPackagesView

        [Test]
        public void CanOpenManagePackagesDialogAndWindowIsOwned()
        {
            Vm.OnRequestManagePackagesDialog(null, null);

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
            Vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
            AssertWindowClosedWithDynamoView<InstalledPackagesView>();

        }

        #endregion

        #region PackageManagerSearchView

        [Test]
        public void CanOpenPackageSearchDialogAndWindowIsOwned()
        {
            Vm.OnRequestPackageManagerSearchDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void CannotCreateDuplicatePackageSearchDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                Vm.OnRequestPackageManagerSearchDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test]
        public void PackageSearchDialogClosesWithDynamo()
        {
            Vm.OnRequestPackageManagerSearchDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
            AssertWindowClosedWithDynamoView<PackageManagerSearchView>();

        }

        #endregion

    }
}
