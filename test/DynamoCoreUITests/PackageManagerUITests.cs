using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class PackageManagerUITests : DynamoTestUI
    {
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

        [Test, Category("Failing")]
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

        [Test, Category("Failing")]
        public void PackagePublishWindowClosesWithDynamo()
        {
            var l = new PublishPackageViewModel(dynSettings.PackageManagerClient);
            Vm.OnRequestPackagePublishDialog(l);

            AssertWindowOwnedByDynamoView<PackageManagerPublishView>();
            AssertWindowClosedWithDynamoView<PackageManagerPublishView>();

        }

        #endregion

        #region InstalledPackagesView

        [Test, Category("Failing")]
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

        [Test, Category("Failing")]
        public void ManagePackagesDialogClosesWithDynamo()
        {
            Vm.OnRequestManagePackagesDialog(null, null);

            AssertWindowOwnedByDynamoView<InstalledPackagesView>();
            AssertWindowClosedWithDynamoView<InstalledPackagesView>();

        }

        #endregion

        #region PackageManagerSearchView

        [Test, Category("Failing")]
        public void CanOpenPackageSearchDialogAndWindowIsOwned()
        {
            Vm.OnRequestPackageManagerSearchDialog(null, null);
            Thread.Sleep(500);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test, Category("Failing")]
        public void CannotCreateDuplicatePackageSearchDialogs()
        {
            for (var i = 0; i < 10; i++)
            {
                Vm.OnRequestPackageManagerSearchDialog(null, null);
            }

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
        }

        [Test, Category("Failing")]
        public void PackageSearchDialogClosesWithDynamo()
        {
            Vm.OnRequestPackageManagerSearchDialog(null, null);

            AssertWindowOwnedByDynamoView<PackageManagerSearchView>();
            AssertWindowClosedWithDynamoView<PackageManagerSearchView>();

        }

        #endregion

    }
}
