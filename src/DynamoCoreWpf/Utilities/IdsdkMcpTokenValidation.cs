using System;
using System.Collections.Generic;
using System.IO;
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
        /// Bearer-token validation cannot run in this process. Every MCP tool call will be
        /// rejected with HTTP 401. See <see cref="McpTokenValidationUnavailableReason"/> for
        /// which of the two causes applies.
        /// </summary>
        Unavailable,

        /// <summary>
        /// AdskIdentitySDK.dll is not mapped into this process, so the capability cannot be
        /// determined. Callers must treat this as "don't know", never as a failure.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Why MCP bearer-token validation is unavailable. The two causes are independent and
    /// produce different guidance, so they are reported separately rather than collapsed
    /// into a single message that would be wrong for one of them.
    /// </summary>
    internal enum McpTokenValidationUnavailableReason
    {
        /// <summary>Validation is available, or availability could not be determined.</summary>
        None,

        /// <summary>
        /// AdskIdentitySDK.dll is mapped but does not export the MCP validation entry point.
        /// The DYN-10773 cause: an IDSDK build predating the MCP validation API.
        /// </summary>
        IdsdkExportMissing,

        /// <summary>
        /// AdpSDKIdentityWrapper.dll cannot be found at all, so the ADP Desktop SDK is not
        /// installed. DynamoMCP P/Invokes that wrapper directly; without it Tier 3 validation
        /// never runs, whatever state IDSDK itself is in.
        /// </summary>
        AdpWrapperMissing
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
    /// <para>
    /// The export check alone has a blind spot, which is what DYN-10778 adds the wrapper check
    /// for: if the ADP Desktop SDK is not installed, <c>AdpSDKIdentityWrapper.dll</c> never
    /// resolves and validation is dead — but IDSDK itself is perfectly healthy and exports the
    /// entry point, so the export probe reports <see cref="McpTokenValidationAvailability.Available"/>.
    /// </para>
    /// </summary>
    internal static class IdsdkMcpTokenValidation
    {
        /// <summary>Base name of the native Autodesk Identity library.</summary>
        internal const string IdsdkModuleName = "AdskIdentitySDK.dll";

        /// <summary>
        /// The ADP Desktop SDK wrapper DynamoMCP P/Invokes to reach IDSDK. Ships with the ADP
        /// Desktop SDK, not with Dynamo.
        /// </summary>
        internal const string AdpWrapperModuleName = "AdpSDKIdentityWrapper.dll";

        /// <summary>
        /// The IDSDK export DynamoMCP's Tier 3 validation ultimately depends on. Introduced in
        /// IDSDK 1.17.0; absent from the 1.16.5.1 build Dynamo 4.2.0 originally shipped.
        /// </summary>
        internal const string McpValidateTokenExport = "idsdk_mcp_validate_token";

        private static readonly object syncRoot = new object();

        // Only a permanently-definitive result is cached. Unknown means AdskIdentitySDK.dll was
        // not mapped yet, and AdpWrapperMissing means a file was not on disk yet — both can
        // change later in the session, so they must stay re-probeable.
        private static (McpTokenValidationAvailability Availability, McpTokenValidationUnavailableReason Reason)? cachedResult;

        private static McpTokenValidationAvailability? testOverride;
        private static McpTokenValidationUnavailableReason testOverrideReason;

        /// <summary>
        /// Returns whether IDSDK in this process can service an MCP token validation call.
        /// Cheap and safe to call repeatedly; a permanently-definitive answer is computed once
        /// and cached.
        /// </summary>
        internal static McpTokenValidationAvailability GetAvailability() => Evaluate().Availability;

        /// <summary>
        /// Why validation is unavailable, for the caller's log message.
        /// <see cref="McpTokenValidationUnavailableReason.None"/> when validation is available
        /// or availability could not be determined.
        /// </summary>
        internal static McpTokenValidationUnavailableReason GetUnavailableReason() => Evaluate().Reason;

        private static (McpTokenValidationAvailability Availability, McpTokenValidationUnavailableReason Reason) Evaluate()
        {
            lock (syncRoot)
            {
                if (testOverride.HasValue)
                {
                    return (testOverride.Value,
                        testOverride.Value == McpTokenValidationAvailability.Unavailable
                            ? testOverrideReason
                            : McpTokenValidationUnavailableReason.None);
                }

                if (cachedResult.HasValue)
                {
                    return cachedResult.Value;
                }

                var result = Probe();

                // A missing DLL can be installed, and an unmapped IDSDK can be mapped, without
                // restarting Dynamo. Only "mapped but no export" is settled for the session.
                if (result.Reason == McpTokenValidationUnavailableReason.IdsdkExportMissing ||
                    result.Availability == McpTokenValidationAvailability.Available)
                {
                    cachedResult = result;
                }

                return result;
            }
        }

        /// <summary>
        /// Test seam. Forces <see cref="GetAvailability"/> to report the supplied value, or
        /// restores real probing when passed <c>null</c>. Also clears the cached probe result
        /// so a test cannot be contaminated by an earlier one.
        /// </summary>
        /// <param name="availability">Value to report, or <c>null</c> to resume real probing.</param>
        /// <param name="reason">Reason to report alongside
        /// <see cref="McpTokenValidationAvailability.Unavailable"/>; ignored otherwise.</param>
        internal static void SetAvailabilityForTesting(
            McpTokenValidationAvailability? availability,
            McpTokenValidationUnavailableReason reason = McpTokenValidationUnavailableReason.IdsdkExportMissing)
        {
            lock (syncRoot)
            {
                testOverride = availability;
                testOverrideReason = reason;
                cachedResult = null;
            }
        }

        private static (McpTokenValidationAvailability Availability, McpTokenValidationUnavailableReason Reason) Probe()
        {
            try
            {
                // Checked first, and independently of IDSDK's own state: DynamoMCP P/Invokes the
                // wrapper, so if the wrapper cannot be found nothing downstream matters. This is
                // the failure mode the export check alone cannot see — IDSDK is healthy and
                // exports the entry point, but there is no wrapper to reach it through.
                if (!IsAdpWrapperResolvable())
                {
                    return (McpTokenValidationAvailability.Unavailable,
                        McpTokenValidationUnavailableReason.AdpWrapperMissing);
                }

                var module = GetModuleHandle(IdsdkModuleName);
                if (module == IntPtr.Zero)
                {
                    // Nothing has mapped IDSDK yet (no auth provider, or a host that never
                    // initializes it). We cannot tell, so we must not block anything.
                    return (McpTokenValidationAvailability.Unknown,
                        McpTokenValidationUnavailableReason.None);
                }

                return GetProcAddress(module, McpValidateTokenExport) != IntPtr.Zero
                    ? (McpTokenValidationAvailability.Available, McpTokenValidationUnavailableReason.None)
                    : (McpTokenValidationAvailability.Unavailable, McpTokenValidationUnavailableReason.IdsdkExportMissing);
            }
            catch (Exception)
            {
                // A probe that cannot run is not evidence that validation is broken.
                return (McpTokenValidationAvailability.Unknown, McpTokenValidationUnavailableReason.None);
            }
        }

        /// <summary>
        /// Whether <c>AdpSDKIdentityWrapper.dll</c> could be loaded if something asked for it.
        /// <para>
        /// Deliberately looks the DLL up rather than loading it. Loading would map a new module
        /// into the process purely to run a diagnostic, and load order is exactly what went
        /// wrong in DYN-10773 — a probe must not be the thing that changes the answer.
        /// </para>
        /// <para>
        /// Mirrors the search DynamoMCP's own <c>ResolveWrapperLibrary</c> performs: the default
        /// OS search order first, then the canonical ADP Desktop SDK install path. Only a miss on
        /// every one of them is treated as definitive, because this verdict withholds a feature.
        /// </para>
        /// </summary>
        private static bool IsAdpWrapperResolvable()
        {
            // Already mapped — e.g. Autodesk Assistant is up and the ADP SDK pulled it in.
            if (GetModuleHandle(AdpWrapperModuleName) != IntPtr.Zero)
            {
                return true;
            }

            foreach (var directory in WrapperSearchDirectories())
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(directory) &&
                        File.Exists(Path.Combine(directory, AdpWrapperModuleName)))
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // A malformed PATH entry is not evidence of anything; skip it.
                }
            }

            return false;
        }

        /// <summary>
        /// Directories a base-name DLL load would search, in roughly the order Windows uses,
        /// plus the ADP Desktop SDK's canonical install location.
        /// </summary>
        private static IEnumerable<string> WrapperSearchDirectories()
        {
            yield return AppContext.BaseDirectory;
            yield return AppDomain.CurrentDomain.BaseDirectory;

            string systemDirectory = null;
            try { systemDirectory = Environment.SystemDirectory; } catch (Exception) { }
            if (systemDirectory != null)
            {
                yield return systemDirectory;
            }

            // C:\Program Files\Common Files\Autodesk\AdpDesktopSDK\bin — not on the default DLL
            // search order, which is why DynamoMCP installs a resolver to reach it explicitly.
            string adpPath = null;
            try
            {
                var commonFiles = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                if (!string.IsNullOrEmpty(commonFiles))
                {
                    adpPath = Path.Combine(commonFiles, "Autodesk", "AdpDesktopSDK", "bin");
                }
            }
            catch (Exception) { }
            if (adpPath != null)
            {
                yield return adpPath;
            }

            string path = null;
            try { path = Environment.GetEnvironmentVariable("PATH"); } catch (Exception) { }
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var entry in path.Split(Path.PathSeparator))
                {
                    yield return entry.Trim();
                }
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
