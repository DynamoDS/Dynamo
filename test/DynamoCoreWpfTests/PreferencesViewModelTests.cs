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

            // Simulate opening preference dialog
            preferencesVM.InitPackagePathsForInstall();

            // The default selected package path for download is AppData.
            var selectedPath = preferencesVM.SelectedPackagePathForInstall;
            Assert.AreEqual(GetAppDataFolder(), selectedPath);

            // Simulating user selection of package path to download packages.
            // This setter also affects the default package directory of the PackageLoader.
            preferencesVM.SelectedPackagePathForInstall = Path.GetTempPath();

            // Simulate exiting preference dialog
            preferencesVM.CommitPackagePathsForInstall();

            var pathManager = ViewModel.Model.PathManager;
            Assert.AreEqual(Path.GetTempPath(), pathManager.DefaultPackagesDirectory);
        }
    }
}
