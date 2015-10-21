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
            Assert.AreEqual(ViewModel.BackgroundPreviewViewModel.Active,ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = false;
            Assert.False(ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            ViewModel.BackgroundPreviewViewModel.Active = true;
            Assert.True(ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);
        }
    }
}
