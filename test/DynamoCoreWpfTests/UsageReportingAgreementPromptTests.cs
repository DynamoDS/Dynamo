using System;
using System.Windows.Interop;
using Dynamo.UI.Prompts;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Covers DYN-10713: the ADP Privacy Settings dialog must be hosted by the
    /// Agreements for Data Collection prompt, not by the Dynamo main window,
    /// otherwise it opens behind the prompt.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public class UsageReportingAgreementPromptTests : DynamoTestUIBase
    {
        [Test]
        public void WhenConfiguringAnalyticsThenAdpDialogIsHostedByThePromptNotTheMainWindow()
        {
            // Arrange - same owner relationship UsageReportingManager sets up,
            // shown non-modally so the test thread is not blocked.
            var prompt = new UsageReportingAgreementPrompt(null, ViewModel) { Owner = View };
            try
            {
                prompt.Show();

                var promptHandle = new WindowInteropHelper(prompt).Handle;
                var mainWindowHandle = new WindowInteropHelper(View).Handle;
                Assert.AreNotEqual(IntPtr.Zero, promptHandle);
                Assert.AreNotEqual(IntPtr.Zero, mainWindowHandle);

                // Act
                var host = prompt.GetADPConsentDialogHost();

                // Assert
                Assert.AreEqual(promptHandle, host);
                Assert.AreNotEqual(mainWindowHandle, host);
            }
            finally
            {
                prompt.Close();
            }
        }
    }
}
