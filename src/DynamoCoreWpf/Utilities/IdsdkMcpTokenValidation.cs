using System;
using System.Runtime.InteropServices;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Whether Autodesk Identity (IDSDK) in this process is able to service an MCP
    /// bearer-token validation call.
    /// </summary>
    internal enum McpTokenValidationAvailability
    {
        /// <summary>
        /// AdskIdentitySDK.dll is mapped into this process and exports the MCP validation
        /// entry point, so Tier 3 bearer-token validation can run.
        /// </summary>
        Available,

        /// <summary>
        /// AdskIdentitySDK.dll is mapped into this process but does not export the MCP
        /// validation entry point. Every MCP tool call will be rejected with HTTP 401.
        /// </summary>
        Unavailable,

        /// <summary>
        /// AdskIdentitySDK.dll is not mapped into this process, so the capability cannot be
        /// determined. Callers must treat this as "don't know", never as a failure.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Detects whether Autodesk Identity (IDSDK) in this process can validate MCP bearer
    /// tokens, so features that depend on it (Autodesk Assistant, the DynamoMCP view
    /// extension) can be withheld instead of loading into a state where every tool call
    /// returns HTTP 401.
    /// <para>
    /// DynamoMCP's Tier 3 gate P/Invokes <c>AdpSDKIdentityWrapper.dll</c>, which in turn
    /// reaches IDSDK through a <em>base-name</em> <c>LoadLibraryW("AdskIdentitySDK.dll")</c>.
    /// That binds to whichever <c>AdskIdentitySDK.dll</c> is already mapped into the process —
    /// Dynamo's own copy in Sandbox, the host's copy under Revit. This probe therefore asks
    /// the same question the same way: it inspects the <em>mapped</em> module and checks for
    /// the export, rather than comparing file versions.
    /// </para>
    /// <para>
    /// A version comparison would be wrong here. The MCP validation APIs were introduced in
    /// official IDSDK 1.17.0, but Revit carries an unofficial 1.16.4.7 with the MCP work
    /// backported, while official 1.16.5.1 — a <em>higher</em> version — lacks it entirely.
    /// A higher version number does not imply a superset of the API surface, so only the
    /// presence of the export is a sound signal (DYN-10773).
    /// </para>
    /// </summary>
    internal static class IdsdkMcpTokenValidation
    {
        /// <summary>Base name of the native Autodesk Identity library.</summary>
        internal const string IdsdkModuleName = "AdskIdentitySDK.dll";

        /// <summary>
        /// The IDSDK export DynamoMCP's Tier 3 validation ultimately depends on. Introduced in
        /// IDSDK 1.17.0; absent from the 1.16.5.1 build Dynamo 4.2.0 originally shipped.
        /// </summary>
        internal const string McpValidateTokenExport = "idsdk_mcp_validate_token";

        private static readonly object syncRoot = new object();

        // Only a definitive result is cached. Unknown means AdskIdentitySDK.dll was not mapped
        // yet, which can change later in the session, so it must stay re-probeable.
        private static McpTokenValidationAvailability? cachedAvailability;

        private static McpTokenValidationAvailability? testOverride;

        /// <summary>
        /// Returns whether IDSDK in this process can service an MCP token validation call.
        /// Cheap and safe to call repeatedly; a definitive answer is computed once and cached.
        /// </summary>
        internal static McpTokenValidationAvailability GetAvailability()
        {
            lock (syncRoot)
            {
                if (testOverride.HasValue)
                {
                    return testOverride.Value;
                }

                if (cachedAvailability.HasValue)
                {
                    return cachedAvailability.Value;
                }

                var availability = Probe();
                if (availability != McpTokenValidationAvailability.Unknown)
                {
                    cachedAvailability = availability;
                }

                return availability;
            }
        }

        /// <summary>
        /// Test seam. Forces <see cref="GetAvailability"/> to report the supplied value, or
        /// restores real probing when passed <c>null</c>. Also clears the cached probe result
        /// so a test cannot be contaminated by an earlier one.
        /// </summary>
        /// <param name="availability">Value to report, or <c>null</c> to resume real probing.</param>
        internal static void SetAvailabilityForTesting(McpTokenValidationAvailability? availability)
        {
            lock (syncRoot)
            {
                testOverride = availability;
                cachedAvailability = null;
            }
        }

        private static McpTokenValidationAvailability Probe()
        {
            try
            {
                var module = GetModuleHandle(IdsdkModuleName);
                if (module == IntPtr.Zero)
                {
                    // Nothing has mapped IDSDK yet (no auth provider, or a host that never
                    // initializes it). We cannot tell, so we must not block anything.
                    return McpTokenValidationAvailability.Unknown;
                }

                return GetProcAddress(module, McpValidateTokenExport) != IntPtr.Zero
                    ? McpTokenValidationAvailability.Available
                    : McpTokenValidationAvailability.Unavailable;
            }
            catch (Exception)
            {
                // A probe that cannot run is not evidence that validation is broken.
                return McpTokenValidationAvailability.Unknown;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // GetProcAddress has no Unicode variant — export names are always ANSI. The explicit
        // LPStr keeps CA2101 satisfied without changing the marshaling CharSet.Ansi already picks.
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
    }
}
