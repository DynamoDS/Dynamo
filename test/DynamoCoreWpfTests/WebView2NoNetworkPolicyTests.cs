using System;
using Dynamo.Wpf.Utilities;
using Microsoft.Web.WebView2.Wpf;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Unit tests for the centralized no-network WebView2 startup policy that hardens the hosted
    /// Edge runtime against background networking when Dynamo is launched with --NoNetworkMode.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public class WebView2NoNetworkPolicyTests
    {
        [Test]
        public void WhenNoNetworkModeEnabledThenAdditionalBrowserArgumentsContainDisableBackgroundNetworking()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties();

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: true);

            // Assert
            Assert.AreEqual(WebView2Utilities.NoNetworkAdditionalBrowserArguments, creationProperties.AdditionalBrowserArguments);
            StringAssert.Contains("--disable-background-networking", creationProperties.AdditionalBrowserArguments);
            StringAssert.Contains("--disable-component-update", creationProperties.AdditionalBrowserArguments);
            StringAssert.Contains("--no-pings", creationProperties.AdditionalBrowserArguments);
        }

        [Test]
        public void WhenNoNetworkModeEnabledThenNetworkServiceIsNotDisabled()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties();

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: true);

            // Assert - disabling NetworkService would break rendering of local content, so it must stay enabled.
            StringAssert.DoesNotContain("NetworkService", creationProperties.AdditionalBrowserArguments);
        }

        [Test]
        public void WhenNoNetworkModeDisabledThenAdditionalBrowserArgumentsAreLeftUntouched()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties();

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: false);

            // Assert - default behavior must be preserved when the flag is off.
            Assert.IsNull(creationProperties.AdditionalBrowserArguments);
        }

        [Test]
        public void WhenNoNetworkModeEnabledThenLoggerReceivesDiagnosticMessage()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties();
            string logged = null;

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: true, logFn: msg => logged = msg);

            // Assert
            Assert.IsNotNull(logged);
            StringAssert.Contains("NoNetworkMode", logged);
        }

        [Test]
        public void WhenNoNetworkModeDisabledThenLoggerIsNotInvoked()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties();
            var invoked = false;

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: false, logFn: _ => invoked = true);

            // Assert
            Assert.IsFalse(invoked);
        }

        [Test]
        public void WhenCreationPropertiesNullThenApplyNoNetworkPolicyThrows()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => WebView2Utilities.ApplyNoNetworkPolicy(null, noNetworkMode: true));
        }

        [Test]
        public void GetNoNetworkBrowserArgumentsReturnsArgumentsOnlyWhenEnabled()
        {
            // Assert
            Assert.AreEqual(WebView2Utilities.NoNetworkAdditionalBrowserArguments, WebView2Utilities.GetNoNetworkBrowserArguments(true));
            Assert.IsNull(WebView2Utilities.GetNoNetworkBrowserArguments(false));
        }

        [Test]
        public void WhenNoNetworkModeEnabledThenUserDataFolderIsRedirectedToIsolatedProfile()
        {
            // Arrange
            var creationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = @"C:\Users\test\AppData\Local\Temp\Dynamo\WebView2"
            };

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: true);

            // Assert - the hardened profile must never share a folder with the default-args profile,
            // otherwise CoreWebView2 creation fails with 0x8007139F when both are used at once.
            Assert.AreEqual(@"C:\Users\test\AppData\Local\Temp\Dynamo\WebView2-NoNetwork", creationProperties.UserDataFolder);
        }

        [Test]
        public void WhenNoNetworkModeDisabledThenUserDataFolderIsLeftUntouched()
        {
            // Arrange
            const string original = @"C:\Users\test\AppData\Local\Temp\Dynamo\WebView2";
            var creationProperties = new CoreWebView2CreationProperties { UserDataFolder = original };

            // Act
            WebView2Utilities.ApplyNoNetworkPolicy(creationProperties, noNetworkMode: false);

            // Assert - default behavior must be preserved when the flag is off.
            Assert.AreEqual(original, creationProperties.UserDataFolder);
        }

        [Test]
        public void GetNoNetworkUserDataFolderReturnsIsolatedSiblingFolder()
        {
            // Assert
            Assert.AreEqual(@"C:\data\WebView2-NoNetwork", WebView2Utilities.GetNoNetworkUserDataFolder(@"C:\data\WebView2"));
            // A trailing separator must not produce a nested empty segment.
            Assert.AreEqual(@"C:\data\WebView2-NoNetwork", WebView2Utilities.GetNoNetworkUserDataFolder(@"C:\data\WebView2\"));
        }

        [Test]
        public void GetNoNetworkUserDataFolderReturnsInputWhenNullOrEmpty()
        {
            // Assert - a surface that never set a UserDataFolder must not gain a spurious one.
            Assert.IsNull(WebView2Utilities.GetNoNetworkUserDataFolder(null));
            Assert.AreEqual(string.Empty, WebView2Utilities.GetNoNetworkUserDataFolder(string.Empty));
        }
    }
}
