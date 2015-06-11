using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

using SystemTestServices;

using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;

using NUnit.Framework;


namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PackageManagerUITests : SystemTestBase
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

        #endregion

    }
}
