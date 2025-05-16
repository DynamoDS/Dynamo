using SystemTestServices;
using Dynamo.PackageManager.ViewModels;
using Moq;
using NUnit.Framework;
using Greg.Responses;
using Greg;
using Dynamo.Tests;
using Dynamo.ViewModels;
using System.Collections.Generic;
using static Dynamo.PackageManager.PackageManagerSearchViewModel;
using Dynamo.Controls;
using Dynamo.PackageManager;
using System.Windows;
using System.Windows.Controls.Primitives;
using DynamoCoreWpfTests.Utility;

namespace DynamoCoreWpfTests.PackageManager
{
    internal class PackageManagerControlTests : SystemTestBase
    {
        [Test]
        public void SearchBoxControlTextTests()
        {
            // Setup
            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty, false);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };
            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            var searchBoxControl = new Dynamo.PackageManager.UI.SearchBoxControl();
            searchBoxControl.DataContext = packageManagerSearchViewModel;

            // Arrange
            var searchTextBox = searchBoxControl.SearchTextBox;
            var searchClearButton = searchBoxControl.SearchClearButton;

            Assert.IsTrue(string.IsNullOrEmpty(searchTextBox.Text));
            Assert.AreEqual(searchClearButton.Visibility, System.Windows.Visibility.Collapsed);

            searchTextBox.Text = "Test search string";

            Assert.IsFalse(string.IsNullOrEmpty(searchTextBox.Text));
            Assert.AreEqual(searchClearButton.Visibility, System.Windows.Visibility.Visible);

            searchClearButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            Assert.IsTrue(string.IsNullOrEmpty(searchTextBox.Text));
        }
    }
}
