using System.Collections.Generic;
using Dynamo.Models;

namespace Dynamo.Interfaces
{
    public interface IPreferences
    {
        int ConsoleHeight { get; set; }
        bool ShowConnector { get; set; }
        ConnectorType ConnectorType { get; set; }
        bool FullscreenWatchShowing { get; set; }
        string NumberFormat { get; set; }
        bool IsUsageReportingApproved { get; set; }
        bool IsAnalyticsReportingApproved { get; set; }
        bool IsFirstRun { get; set; }
        double WindowX { get; set; }
        double WindowY { get; set; }
        double WindowH { get; set; }
        double WindowW { get; set; }
        int MaxNumRecentFiles { get; set; }
        List<string> RecentFiles { get; set; }
        List<string> PackageDirectoriesToUninstall { get; set; }

        /// <summary>
        /// Call this method to serialize PreferenceSettings given the output 
        /// file path.
        /// </summary>
        /// <param name="filePath">The full path of the output file to serialize
        /// PreferenceSettings to.</param>
        /// <returns>Returns true if the serialization is successful, or false 
        /// otherwise.</returns>
        bool Save(string filePath);
    }
}
