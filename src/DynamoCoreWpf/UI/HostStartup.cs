using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Dynamo.Configuration;
using Dynamo.Utilities;

namespace Dynamo.Wpf.UI
{
    /// <summary>
    /// Provides static methods and properties for initializing and managing the host startup context during application
    /// launch.
    /// </summary>
    /// <remarks><see cref="HostStartup"/> is intended for use during the application's splash screen or early
    /// startup phase, before other core services are available. It allows initialization of a shared startup context
    /// and provides access to user-specific directories based on the current host environment.</remarks>
    public static class HostStartup
    {
        private static int _initialized;
        public static SplashScreenStartupContext? Current { get; private set; }

        /// <summary>
        /// Initialize the HostStartup with the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool TryInitialize(SplashScreenStartupContext context)
        {
            if (context == null) return false;
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0) return false;
            Current = context;
            return true;
        }

        /// <summary>
        /// Resets the static state of the class to its initial, uninitialized condition.
        /// </summary>
        /// <remarks>Call this method to clear any existing state and reinitialize the class as if it had
        /// not been used.  This is typically used for testing scenarios or to force reinitialization.  This method is
        /// not thread-safe; ensure that no other threads are accessing the class while calling <see
        /// cref="Reset"/>.</remarks>
        public static void Reset()
        {
            Current = null;
            Interlocked.Exchange(ref _initialized, 0);
        }

        /// <summary>
        /// Gets the path to the user-specific directory for the current application version.
        /// </summary>
        /// <remarks>The returned directory path is versioned based on the application's major and minor
        /// version numbers.  If user-specific host information is available, the path is constructed using the host's
        /// user data folder;  otherwise, it defaults to a standard location under the user's application data
        /// folder.</remarks>
        /// <returns>A string containing the full path to the user directory for the current application version.  The directory
        /// may not exist and may need to be created by the caller.</returns>
        public static string GetUserDirectory()
        {
            //we need to use userDataFolder and hostAnalyticsInfo just in SplashScreen due that PathManager/PathResolver are not created yet when SplashScreen is launched from a host
            if (Current != null && !string.IsNullOrEmpty(Current.UserDataFolder) && Current.HostInfo != null)
            {
                var version = Current.HostInfo.Value.HostVersion;

                return Path.Combine(Current.UserDataFolder,
                                String.Format("{0}.{1}", version.Major, version.Minor));
            }
            else
            {
                var version = AssemblyHelper.GetDynamoVersion();

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(Path.Combine(folder, Configurations.DynamoAsString, "Dynamo Core"),
                                String.Format("{0}.{1}", version.Major, version.Minor));
            }

        }
    }
}
