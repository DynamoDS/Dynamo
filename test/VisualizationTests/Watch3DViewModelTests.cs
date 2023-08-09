using Dynamo.Tests;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class Watch3DViewModelTests : DynamoViewModelUnitTest
    {
        [Test]
        public void Watch3DViewModel_Active_InSyncWithPreferences()
        {
            var backgroundPreviewName = ViewModel.BackgroundPreviewViewModel.PreferenceWatchName;
            Assert.AreEqual(ViewModel.BackgroundPreviewViewModel.Active,
                ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));

            ViewModel.BackgroundPreviewViewModel.Active = false;
            Assert.False(ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));

            ViewModel.BackgroundPreviewViewModel.Active = true;
            Assert.True(ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));
        }
        [Test]
        public void Watch3DViewModel_Active_InSyncWithPreferencesUsing1_0API()
        {
            Assert.AreEqual(ViewModel.BackgroundPreviewViewModel.Active, ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = false;
            Assert.False(ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = true;
            Assert.True(ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);
        }
    }
}
