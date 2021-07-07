using System.IO;
using System.Linq;
using Dynamo.GraphMetadata;
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

            var packageLoader = ViewModel.PackageManagerClientViewModel.PackageManagerExtension.PackageLoader;
            Assert.Null(packageLoader.DefaultPackagesDirectory);
        }

        [Test]
        public void CanAddRequiredProperty()
        {
            // Arrange
            var preferenceSettings = this.ViewModel.PreferenceSettings;

            // Act
            preferenceSettings.RequiredProperties.Clear();
            preferenceSettings.AddRequiredProperty(null);

            var requiredProperty = preferenceSettings.RequiredProperties.First();

            // Assert
            Assert.IsNotNull(requiredProperty);
            Assert.That(requiredProperty.Key.Equals("Required Property 1"));
        }

        [Test]
        public void SetRequiredPropertyGlobalValue()
        {
            // Arrange
            var preferenceSettings = this.ViewModel.PreferenceSettings;
            var testGlobalValue = "Global Value";

            // Act
            preferenceSettings.RequiredProperties.Clear();
            
            preferenceSettings.AddRequiredProperty(null);
            var preferenceSettingsRequiredProperty = preferenceSettings.RequiredProperties.First();

            preferenceSettingsRequiredProperty.ValueIsGlobal = true;
            preferenceSettingsRequiredProperty.GlobalValue = testGlobalValue;
            
            // Assert
            Assert.That(preferenceSettingsRequiredProperty.GlobalValue.Equals(testGlobalValue));
            Assert.That(preferenceSettingsRequiredProperty.GraphValue.Equals(testGlobalValue));
        }
    }
}
