using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Core;

namespace Dynamo.Utilities
{
    /// <summary>
    /// A view extension blocked by <see cref="LegacyAssistantExtensionGuard"/>, recorded so a
    /// single startup notification/dialog can be raised for it once the main window exists.
    /// </summary>
    internal readonly struct BlockedLegacyViewExtension
    {
        internal string DisplayName { get; }
        internal string ManifestPath { get; }
        internal string AssemblyPath { get; }

        internal BlockedLegacyViewExtension(string displayName, string manifestPath, string assemblyPath)
        {
            DisplayName = displayName;
            ManifestPath = manifestPath;
            AssemblyPath = assemblyPath;
        }
    }

    /// <summary>
    /// DYN-10745 band-aid. Dynamo 4.2 ships Autodesk Assistant and DynamoMCP as built-in
    /// packages for the first time. Older, pre-built-in copies of either extension are still
    /// present on some machines (manual alpha installs, or files orphaned by a Revit
    /// uninstall) and can silently displace the built-in copy or corrupt assembly resolution
    /// order. This guard refuses to load either extension from any location outside Dynamo's
    /// Built-In Packages directory.
    /// Remove this entire type, and its call sites in PackageLoader.ScanPackageDirectory and
    /// ViewExtensionLoader.Load(string), once DYN-10739 lands the permanent architectural fix.
    /// </summary>
    internal static class LegacyAssistantExtensionGuard
    {
        internal const string AutodeskAssistantTypeName = "Dynamo.AutodeskAssistant.AutodeskAssistantViewExtension";
        internal const string McpViewExtensionTypeName = "Dynamo.MCP.McpViewExtension";

        // Autodesk Assistant's package identity churned across DYN-10450
        // (AutodeskAssistant -> DynamoAssistant -> back to AutodeskAssistant). Both names are
        // treated as the same restricted package so a pre-rename install is still caught.
        private static readonly Dictionary<string, string> restrictedPackageDisplayNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AutodeskAssistant", "Autodesk Assistant" },
                { "DynamoAssistant", "Autodesk Assistant" },
                { "DynamoMCP", "DynamoMCP" }
            };

        private static readonly Dictionary<string, string> restrictedTypeDisplayNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { AutodeskAssistantTypeName, "Autodesk Assistant" },
                { McpViewExtensionTypeName, "DynamoMCP" }
            };

        // Package blocks record the package's ROOT DIRECTORY: a Dynamo package is a
        // self-contained folder, so "delete this folder" is safe and correct advice.
        private static readonly HashSet<string> blockedPackageDirectories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // View-extension blocks record the individual manifest/assembly FILE paths, not a
        // folder. A legacy install found this way (e.g. files orphaned directly under a
        // Revit add-in folder by an uninstall) is not necessarily isolated in its own folder,
        // so inferring "delete this folder" from the file's location could tell a user to
        // delete something far broader than intended.
        private static readonly List<BlockedLegacyViewExtension> blockedViewExtensions =
            new List<BlockedLegacyViewExtension>();

        /// <summary>
        /// Whether the guard is active. Set from DynamoModel startup based on
        /// !IsTestMode, so the existing test suite (which routinely loads packages and
        /// extensions from directories outside Built-In Packages) is unaffected.
        /// </summary>
        internal static bool IsEnabled { get; set; }

        /// <summary>
        /// Looks up the friendly display name for a restricted package name. Returns false
        /// for any package name that is not restricted.
        /// </summary>
        internal static bool TryGetRestrictedPackageDisplayName(string packageName, out string displayName)
        {
            displayName = null;
            return packageName != null && restrictedPackageDisplayNames.TryGetValue(packageName, out displayName);
        }

        /// <summary>
        /// Looks up the friendly display name for a restricted view extension TypeName.
        /// Returns false for any TypeName that is not restricted.
        /// </summary>
        internal static bool TryGetRestrictedViewExtensionDisplayName(string typeName, out string displayName)
        {
            displayName = null;
            return typeName != null && restrictedTypeDisplayNames.TryGetValue(typeName, out displayName);
        }

        /// <summary>
        /// True if the given path is not located under Dynamo's Built-In Packages directory.
        /// </summary>
        internal static bool IsOutsideBuiltInPackages(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;

            var builtInDirectory = PathManager.BuiltinPackagesDirectory;
            if (string.IsNullOrEmpty(builtInDirectory)) return true;

            string fullPath;
            string fullBuiltInDirectory;
            try
            {
                fullPath = Path.GetFullPath(path);
                fullBuiltInDirectory = Path.GetFullPath(builtInDirectory)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                return true;
            }

            // A plain StartsWith would let a sibling like "Built-In PackagesOld" pass as if it
            // were under "Built-In Packages" -- require an exact match or a directory-separator
            // boundary right after the prefix.
            return !fullPath.Equals(fullBuiltInDirectory, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.StartsWith(fullBuiltInDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        internal static void RecordBlockedPackage(string directory)
        {
            if (!string.IsNullOrEmpty(directory)) blockedPackageDirectories.Add(directory);
        }

        internal static void RecordBlockedViewExtension(string displayName, string manifestPath, string assemblyPath)
        {
            blockedViewExtensions.Add(new BlockedLegacyViewExtension(displayName, manifestPath, assemblyPath));
        }

        /// <summary>
        /// View extensions blocked at that layer only. A block at the package layer already
        /// raises its own startup notification via LibraryLoadFailedException, so those
        /// packages are intentionally excluded here to avoid a duplicate notification.
        /// </summary>
        internal static IReadOnlyList<BlockedLegacyViewExtension> BlockedViewExtensions => blockedViewExtensions;

        /// <summary>
        /// The union of every path blocked by either gate (package folders and view-extension
        /// files alike), for the consolidated startup dialog.
        /// </summary>
        internal static IReadOnlyCollection<string> AllBlockedPaths =>
            blockedPackageDirectories
                .Concat(blockedViewExtensions.SelectMany(b => new[] { b.ManifestPath, b.AssemblyPath }))
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        internal static bool HasBlockedPaths =>
            blockedPackageDirectories.Count > 0 || blockedViewExtensions.Count > 0;

        /// <summary>
        /// Clears all recorded state. Called once per DynamoModel construction so repeated
        /// model instances in the same process (tests, multi-instance hosts) don't leak state
        /// from a previous instance.
        /// </summary>
        internal static void Reset()
        {
            blockedPackageDirectories.Clear();
            blockedViewExtensions.Clear();
        }
    }
}
