using System.IO;
using System.Linq;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Tests;
using NUnit.Framework;
using TestServices;

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

        [Test]
        public void SecurityPrefs_DisableTrustWarnings()
        {
            Assert.IsFalse(ViewModel.PreferenceSettings.DisableTrustWarnings);
            //assert model setter is no-op
            ViewModel.PreferenceSettings.DisableTrustWarnings = true;
            Assert.IsFalse(ViewModel.PreferenceSettings.DisableTrustWarnings);

            //assert model set method works
            ViewModel.PreferenceSettings.SetTrustWarningsDisabled(true);
            Assert.True(ViewModel.PreferenceSettings.DisableTrustWarnings);
        }

        [Test]
        public void TestGetTransformedHostUnits()
        {
            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Millimeters),
                Dynamo.Configuration.Configurations.Units.Meters);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Centimeters),
                Dynamo.Configuration.Configurations.Units.Centimeters);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Meters),
                Dynamo.Configuration.Configurations.Units.Millimeters);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Kilometers),
                Dynamo.Configuration.Configurations.Units.Kilometers);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Feet),
                Dynamo.Configuration.Configurations.Units.Feet);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Inches),
                Dynamo.Configuration.Configurations.Units.Inches);

            Assert.AreEqual(ViewModel.PreferencesViewModel.GetTransformedHostUnits(Dynamo.Configuration.Configurations.Units.Miles),
                Dynamo.Configuration.Configurations.Units.Miles);
        }

        [Test]
        public void PathManagerWithDifferentHostTest()
        {
            PathManager singletonPathManager = PathManager.Instance;
            TestPathResolverParams revitResolverParams = new TestPathResolverParams()
            {
                UserDataRootFolder = "C:\\Users\\user\\AppData\\Roaming\\Dynamo\\Dynamo Revit",
                CommonDataRootFolder = "C:\\ProgramData\\Autodesk\\RVT 2024\\Dynamo"
            };

            IPathResolver revitPathResolver = new TestPathResolver(revitResolverParams);
            string dynamoRevitHostPath = "C:\\Program Files\\Autodesk\\Revit 2024\\AddIns\\DynamoForRevit\\Revit)";
            singletonPathManager.AssignHostPathAndIPathResolver(dynamoRevitHostPath, revitPathResolver);

            string dynamoRevitUserDataDirectory = Path.Combine(revitResolverParams.UserDataRootFolder, "3.6");
            string dynamoRevitCommonDataDirectory = Path.Combine(revitResolverParams.CommonDataRootFolder, "3.6");
            string dynamoRevitSamplesPath = Path.Combine(revitResolverParams.CommonDataRootFolder, "samples\\en-US");
            string dynamoRevitTemplatesPath = Path.Combine(revitResolverParams.CommonDataRootFolder, "templates\\en-US");

            Assert.AreEqual(Path.GetFullPath(singletonPathManager.UserDataDirectory), Path.GetFullPath(dynamoRevitUserDataDirectory));
            Assert.AreEqual(Path.GetFullPath(singletonPathManager.CommonDataDirectory), Path.GetFullPath(dynamoRevitCommonDataDirectory));
            Assert.AreEqual(Path.GetFullPath(singletonPathManager.SamplesDirectory), Path.GetFullPath(dynamoRevitSamplesPath));
            Assert.AreEqual(Path.GetFullPath(singletonPathManager.DefaultTemplatesDirectory), Path.GetFullPath(dynamoRevitTemplatesPath));
        }

        [Test]
        public void EnsureTemplatePathsAreSetAndCanBeUpdated()
        {
            PathManager pathManager = PathManager.Instance;
            Assert.IsFalse(string.IsNullOrEmpty(ViewModel.PreferencesViewModel.TemplateLocation));
            Assert.IsFalse(string.IsNullOrEmpty(pathManager.DefaultTemplatesDirectory));
            Assert.IsTrue(ViewModel.Model.IsDefaultPreferenceItemLocation(PathManager.PreferenceItem.Templates));
            Assert.IsFalse(ViewModel.PreferencesViewModel.CanResetTemplateLocation);

            // Set a new template location
            ViewModel.Model.UpdatePreferenceItemLocation(PathManager.PreferenceItem.Templates, Path.GetTempPath());
            Assert.AreEqual(Path.GetTempPath(), ViewModel.PreferencesViewModel.TemplateLocation);
            Assert.AreEqual(Path.GetTempPath(), ViewModel.PreferenceSettings.TemplateFilePath);
            Assert.IsTrue(ViewModel.PreferencesViewModel.CanResetTemplateLocation);

            // Reset the template location
            ViewModel.Model.ResetPreferenceItemLocation(PathManager.PreferenceItem.Templates);
            Assert.IsTrue(ViewModel.Model.IsDefaultPreferenceItemLocation(PathManager.PreferenceItem.Templates));
            Assert.IsFalse(ViewModel.PreferencesViewModel.CanResetTemplateLocation);
        }
    }
}
