using System.IO;
using System.Linq;
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

        [Test]
        public void PackagePathForInstall_Add_Update_Remove_Paths()
        {
            var preferencesVM = ViewModel.PreferencesViewModel;
            Assert.NotNull(preferencesVM);

            // Simulate opening preference dialog
            preferencesVM.PackagePathsViewModel.InitializeRootLocations();
            preferencesVM.InitPackagePathsForInstall();

            // The default selected package path for download is AppData.
            var selectedPath = preferencesVM.SelectedPackagePathForInstall;
            Assert.AreEqual(GetAppDataFolder(), selectedPath);

            // Add a new path
            preferencesVM.PackagePathsViewModel.RootLocations.Add(@"C:\");

            // Check that the path is added to the list of paths for install
            Assert.AreEqual(@"C:\", preferencesVM.PackagePathsForInstall.Last());

            // Update path
            preferencesVM.SelectedPackagePathForInstall = @"C:\";
            preferencesVM.PackagePathsViewModel.RootLocations[
                preferencesVM.PackagePathsViewModel.RootLocations.IndexOf(@"C:\")] = @"D:\";

            // Check that directory has been updated in the list of paths for install
            Assert.AreEqual(@"D:\", preferencesVM.PackagePathsForInstall.Last());

            // Check that the selection has updated
            Assert.AreEqual(@"D:\", preferencesVM.SelectedPackagePathForInstall);

            // Remove path
            preferencesVM.PackagePathsViewModel.RootLocations.Remove(@"D:\");

            // Check that the selection has changed and now points to default AppData.
            var selection = preferencesVM.SelectedPackagePathForInstall;
            Assert.AreEqual(GetAppDataFolder(), selection);

            // Check that the path is not in the list
            Assert.IsFalse(preferencesVM.PackagePathsForInstall.Contains(@"D:\"));

            // Simulate exiting preference dialog
            preferencesVM.CommitPackagePathsForInstall();
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
