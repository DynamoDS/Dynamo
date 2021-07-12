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

        [Test]
        public void PackagePathsForInstall_RetainsPathThatDoesNotExist()
        {
            //add a new path to the package paths
            ViewModel.Model.PreferenceSettings.CustomPackageFolders.Add(@"C:\DoesNotExist\");
            //set to null, so getter regenerates list
            ViewModel.PreferencesViewModel.PackagePathsForInstall = null;
            //access getter
            var paths = ViewModel.PreferencesViewModel.PackagePathsForInstall;
            Assert.Contains(@"C:\DoesNotExist\", paths);
        }
        [Test]
        public void PackagePathsForInstall_FiltersSomeFilePaths()
        { 
            //add a new path to the package paths
            ViewModel.Model.PreferenceSettings.CustomPackageFolders.Add(@"C:\DoesNotExist\DoesNotExist.DLL");
            //set to null, so getter regenerates list
            ViewModel.PreferencesViewModel.PackagePathsForInstall = null;
            //access getter
            var paths = ViewModel.PreferencesViewModel.PackagePathsForInstall;
            Assert.False(paths.Contains(@"C:\DoesNotExist\DoesNotExist.DLL"));
        }
    }
}
