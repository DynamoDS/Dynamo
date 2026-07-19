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
    }
}
