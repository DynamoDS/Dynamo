using System.IO;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class PreferencesViewModelTests : DynamoViewModelUnitTest
    {
        [Test]
        public void SelectedPackagePathForInstall_Setter()
        {
            var preferencesVM = ViewModel.PreferencesViewModel;
            Assert.NotNull(preferencesVM);

            // The default selected package path for download is AppData.
            var selectedPath = preferencesVM.SelectedPackagePathForInstall;
            Assert.AreEqual(GetAppDataFolder(), selectedPath);

            // Simulating user selection of package path to download packages.
            // This setter also affects the default package directory of the PackageLoader.
            preferencesVM.SelectedPackagePathForInstall = Path.GetTempPath();

            var pathManager = ViewModel.Model.PathManager;
            Assert.AreEqual(Path.GetTempPath(), pathManager.DefaultPackagesDirectory);
        }
    }
}
