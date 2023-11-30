using Dynamo.Configuration;
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
                PreferenceSettings.Instance.GetIsBackgroundPreviewActive(backgroundPreviewName));

            ViewModel.BackgroundPreviewViewModel.Active = false;
            Assert.False(PreferenceSettings.Instance.GetIsBackgroundPreviewActive(backgroundPreviewName));

            ViewModel.BackgroundPreviewViewModel.Active = true;
            Assert.True(PreferenceSettings.Instance.GetIsBackgroundPreviewActive(backgroundPreviewName));
        }
        [Test]
        public void Watch3DViewModel_Active_InSyncWithPreferencesUsing1_0API()
        {
            Assert.AreEqual(ViewModel.BackgroundPreviewViewModel.Active, PreferenceSettings.Instance.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = false;
            Assert.False(PreferenceSettings.Instance.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = true;
            Assert.True(PreferenceSettings.Instance.IsBackgroundPreviewActive);
        }
    }
}
