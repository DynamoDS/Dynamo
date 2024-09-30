using System.Windows.Controls.Primitives;
using Dynamo.Wpf.Views;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class SecTabUITests: DynamoTestUIBase
    {
        [Test]
        public void TestDisableTrustWarningToggle()
        {
            var preferencesWindow = new PreferencesView(View);
            preferencesWindow.Show();
            DispatcherUtil.DoEvents();
            var trustToggle = preferencesWindow.FindName("TrustWarningToggle") as ToggleButton;
            Assert.IsFalse(trustToggle.IsChecked.Value);
            trustToggle.IsChecked = true;
            DispatcherUtil.DoEvents();
            Assert.IsTrue(trustToggle.IsChecked.Value);
            Assert.IsTrue(Model.PreferenceSettings.DisableTrustWarnings);
            Assert.IsTrue(ViewModel.PreferencesViewModel.DisableTrustWarnings);
            trustToggle.IsChecked = false;
            DispatcherUtil.DoEvents();
            Assert.IsFalse(trustToggle.IsChecked.Value);
            Assert.IsFalse(Model.PreferenceSettings.DisableTrustWarnings);
            Assert.IsFalse(ViewModel.PreferencesViewModel.DisableTrustWarnings);

        }
    }
}
