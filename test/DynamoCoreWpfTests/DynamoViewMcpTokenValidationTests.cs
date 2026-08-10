using System;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Utilities;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Covers the DYN-10775 gate: when Autodesk Identity (IDSDK) in this process cannot validate
    /// MCP bearer tokens, Autodesk Assistant and the MCP view extension must not be offered,
    /// because every MCP tool call would be rejected with HTTP 401 (DYN-10773).
    /// <para>
    /// These tests drive <see cref="IdsdkMcpTokenValidation"/> through its test seam rather than
    /// relying on the machine's real IDSDK. That is deliberate: the sibling
    /// <see cref="DynamoViewNoNetworkModeTests"/> fixture has no AuthProvider, so IDSDK is never
    /// mapped into the process and a real probe would always report
    /// <see cref="McpTokenValidationAvailability.Unknown"/> — which by design blocks nothing.
    /// </para>
    /// </summary>
    public class DynamoViewMcpTokenValidationTests : DynamoTestUIBase
    {
        private const string UnrelatedExtensionId = "11111111-1111-1111-1111-111111111111";

        [TearDown]
        public void ResetMcpTokenValidationProbe()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(null);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenAutodeskAssistantExtensionIsDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant",
                "added");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenMcpViewExtensionIsDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.McpViewExtensionId,
                "Dynamo MCP View Extension",
                "added");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsAvailableThenAutodeskAssistantExtensionIsNotDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Available);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant",
                "added");

            Assert.IsFalse(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnknownThenAutodeskAssistantExtensionIsNotDisabled()
        {
            // Unknown means AdskIdentitySDK.dll is not mapped into the process, so nothing can be
            // concluded about the MCP validation API. The gate must fail open rather than withhold
            // the Assistant on a guess.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unknown);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant",
                "added");

            Assert.IsFalse(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenUnrelatedExtensionIsNotDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                UnrelatedExtensionId,
                "Some Other Extension",
                "added");

            Assert.IsFalse(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenAssistantTabCannotBeAddedViaSideBar()
        {
            // Covers the late-add path an extension reaches through
            // ViewLoadedParams.AddToExtensionsSideBar(), which bypasses the extension-load guards.
            //
            // This fixture does not enable NoNetworkMode and its DynamoModel has no AuthProvider
            // (so AuthenticationManager.IsIDSDKInitialized() is unconditionally true), which means
            // neither of the other two guards in AddOrFocusExtensionControl can be the reason the
            // call is blocked — only the MCP token validation gate can.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var stubExtension = new StubViewExtension(DynamoView.AutodeskAssistantExtensionId);
            var result = View.AddOrFocusExtensionControl(stubExtension, null);

            Assert.AreEqual(DynamoView.ExtensionControlResult.Blocked, result);
            Assert.IsFalse(ViewModel.SideBarTabItems
                .OfType<System.Windows.Controls.TabItem>()
                .Any(t => string.Equals(t.Uid, DynamoView.AutodeskAssistantExtensionId,
                    StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenViewLoadedParamsReportsItUnavailable()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var loadedParams = new ViewLoadedParams(View, ViewModel);

            Assert.IsFalse(loadedParams.IsMcpTokenValidationAvailable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnknownThenViewLoadedParamsReportsItAvailable()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unknown);

            var loadedParams = new ViewLoadedParams(View, ViewModel);

            Assert.IsTrue(loadedParams.IsMcpTokenValidationAvailable);
        }

        [Test]
        public void WhenTestOverrideIsChangedThenTheCachedResultDoesNotShadowIt()
        {
            // Guards the seam itself. GetAvailability() caches a definitive answer, so a stale cache
            // would make one test's setup silently determine the next test's outcome.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);
            Assert.AreEqual(McpTokenValidationAvailability.Unavailable, IdsdkMcpTokenValidation.GetAvailability());

            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Available);
            Assert.AreEqual(McpTokenValidationAvailability.Available, IdsdkMcpTokenValidation.GetAvailability());
        }

        private class StubViewExtension : IViewExtension
        {
            public StubViewExtension(string uniqueId) { UniqueId = uniqueId; }
            public string UniqueId { get; }
            public string Name => "Stub";
            public void Startup(ViewStartupParams p) { }
            public void Loaded(ViewLoadedParams p) { }
            public void Shutdown() { }
            public void Dispose() { }
        }
    }
}
