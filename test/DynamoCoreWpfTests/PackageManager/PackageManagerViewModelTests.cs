using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Moq;
using NUnit.Framework;
using SystemTestServices;
using Dynamo.Wpf.Views;
using Dynamo.Core;
using Dynamo.Extensions;
using System.Reflection;
using System.Threading.Tasks;
using Dynamo.UI.Prompts;
using System.Windows.Controls.Primitives;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace DynamoCoreWpfTests.PackageManager
{
    internal class PackageManagerViewModelTests : SystemTestBase
    {
        [Test]
        public void PackageManagerLoadAllViewModelsTests()
        {
            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true }; ;

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            var packageManagerViewModel = new PackageManagerViewModel(ViewModel, packageManagerSearchViewModel);

            var filters = packageManagerViewModel.Filters;
            var localPackages = packageManagerViewModel.LocalPackages;
            var preferenceViewModel = packageManagerViewModel.PreferencesViewModel;
            var publishPackageViewModel = packageManagerViewModel.PubPkgVM;

            Assert.IsNotNull(filters);
            Assert.IsNotNull(localPackages);
            Assert.IsNotNull(preferenceViewModel);
            Assert.IsNotNull(publishPackageViewModel);
        }
    }
}
