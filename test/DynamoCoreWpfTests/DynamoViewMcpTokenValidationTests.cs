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
    /// MCP bearer tokens, the Autodesk Assistant and MCP view extension panels must not be opened
    /// or auto-re-opened, because every MCP tool call would be rejected with HTTP 401 (DYN-10773).
    /// The extensions still load and register their own UI; only opening a panel is blocked.
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

        // Reset before each test as well as after. The probe result is process-global static
        // state, so anything else in this assembly that sets it and fails to clean up would
        // otherwise silently decide these tests' outcomes. NUnit runs the base fixture's
        // [SetUp] Start() first, then this.
        [SetUp]
        public void ResetMcpTokenValidationProbeBeforeTest()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(null);
        }

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
                "Autodesk Assistant");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenMcpViewExtensionIsDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.McpViewExtensionId,
                "Dynamo MCP View Extension");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsAvailableThenAutodeskAssistantExtensionIsNotDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Available);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant");

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
                "Autodesk Assistant");

            Assert.IsFalse(shouldDisable);
        }

        [Test]
        public void WhenMcpTokenValidationIsUnavailableThenUnrelatedExtensionIsNotDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(McpTokenValidationAvailability.Unavailable);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                UnrelatedExtensionId,
                "Some Other Extension");

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

            // Real content rather than null: if the gate ever stops blocking, AddOrFocusExtensionControl
            // goes on to use it, and a NullReferenceException here would mask the actual regression
            // (the gate letting the extension through) behind a confusing failure.
            var stubExtension = new StubViewExtension(DynamoView.AutodeskAssistantExtensionId);
            var result = View.AddOrFocusExtensionControl(stubExtension, new System.Windows.Controls.ContentControl());

            Assert.AreEqual(DynamoView.ExtensionControlResult.Blocked, result);
            Assert.IsFalse(ViewModel.SideBarTabItems
                .OfType<System.Windows.Controls.TabItem>()
                .Any(t => string.Equals(t.Uid, DynamoView.AutodeskAssistantExtensionId,
                    StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void WhenAdpWrapperIsMissingThenAutodeskAssistantExtensionIsDisabled()
        {
            // DYN-10778 AC 6. The export probe cannot see this case: IDSDK itself is healthy and
            // exports idsdk_mcp_validate_token, but the ADP Desktop SDK is not installed, so
            // AdpSDKIdentityWrapper.dll — the library DynamoMCP actually P/Invokes — never
            // resolves and every MCP call is rejected with HTTP 401 regardless.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Unavailable,
                McpTokenValidationUnavailableReason.AdpWrapperMissing);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void WhenAdpWrapperIsMissingThenTheReasonIsReportedSeparately()
        {
            // The two causes need different guidance — an out-of-date Identity Manager versus a
            // missing ADP Desktop SDK install. Collapsing them would send whoever reads the log
            // after the wrong thing.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Unavailable,
                McpTokenValidationUnavailableReason.AdpWrapperMissing);

            Assert.AreEqual(McpTokenValidationUnavailableReason.AdpWrapperMissing,
                IdsdkMcpTokenValidation.GetUnavailableReason());

            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Unavailable,
                McpTokenValidationUnavailableReason.IdsdkExportMissing);

            Assert.AreEqual(McpTokenValidationUnavailableReason.IdsdkExportMissing,
                IdsdkMcpTokenValidation.GetUnavailableReason());
        }

        [Test]
        public void WhenValidationIsAvailableOrUnknownThenNoUnavailableReasonIsReported()
        {
            // A reason only means something alongside Unavailable. Leaking a stale one would let
            // a caller log "ADP SDK missing" for a process where validation works.
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Available,
                McpTokenValidationUnavailableReason.AdpWrapperMissing);

            Assert.AreEqual(McpTokenValidationUnavailableReason.None,
                IdsdkMcpTokenValidation.GetUnavailableReason());

            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Unknown,
                McpTokenValidationUnavailableReason.AdpWrapperMissing);

            Assert.AreEqual(McpTokenValidationUnavailableReason.None,
                IdsdkMcpTokenValidation.GetUnavailableReason());
        }

        [Test]
        public void WhenAdpWrapperIsMissingThenUnrelatedExtensionIsNotDisabled()
        {
            IdsdkMcpTokenValidation.SetAvailabilityForTesting(
                McpTokenValidationAvailability.Unavailable,
                McpTokenValidationUnavailableReason.AdpWrapperMissing);

            var shouldDisable = View.DisableExtensionWhenMcpTokenValidationUnavailable(
                UnrelatedExtensionId,
                "Some Other Extension");

            Assert.IsFalse(shouldDisable);
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
