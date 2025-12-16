using Dynamo.Models;

namespace Dynamo.Wpf.UI
{
    /// <summary>
    /// Provides contextual information for initializing a splash screen during application startup, including host
    /// analytics data and the user data folder path.
    /// </summary>
    /// <remarks>This context is typically used to supply startup-related information to components that
    /// display or manage a splash screen. It encapsulates optional host analytics details and the location of the
    /// user-specific data directory.</remarks>
    public sealed class SplashScreenStartupContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreenStartupContext"/> class with the specified host
        /// analytics information and user data folder path.
        /// </summary>
        /// <remarks>Use this constructor to provide context information required during the splash screen
        /// startup process, such as analytics data and the location for user-specific files.</remarks>
        /// <param name="hostInfo">The analytics information for the host application, or <see langword="null"/> if not available.</param>
        /// <param name="userDataFolder">The path to the user data folder. If <see langword="null"/>, an empty string is used.</param>
        public SplashScreenStartupContext(HostAnalyticsInfo ?hostInfo, string userDataFolder)
        {
            HostInfo = hostInfo != null? hostInfo:null;
            UserDataFolder = userDataFolder ?? string.Empty;
        }

        /// <summary>
        /// Gets information about the host environment for analytics purposes.
        /// </summary>
        public HostAnalyticsInfo? HostInfo { get; }

        /// <summary>
        /// Gets the path to the user data folder.
        /// </summary>
        public string UserDataFolder { get; }
    }
}
