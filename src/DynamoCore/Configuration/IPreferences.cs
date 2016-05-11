using System;
using System.Collections.Generic;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Models;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// An interface which defines preference settings.
    /// </summary>
    public interface IPreferences
    {
        /// <summary>
        /// Returns height of console
        /// </summary>
        int ConsoleHeight { get; set; }

        /// <summary>
        /// Indicates whether connector should be displayed on canvas or not.
        /// </summary>
        bool ShowConnector { get; set; }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        bool ShowPreviewBubbles { get; set; }

        /// <summary>
        /// Indicates which type of connector's should be displayed on canvas.
        /// I.e. bezier or polyline
        /// </summary>
        ConnectorType ConnectorType { get; set; }

        /// <summary>
        /// Indicates whether background preview is active or not.
        /// </summary>
        bool IsBackgroundPreviewActive { get; set; }

        /// <summary>
        /// Indicates whether background grid is visible or not.
        /// </summary>
        bool IsBackgroundGridVisible { get; set; }

        /// <summary>
        /// Returns the decimal precision used to display numbers.
        /// </summary>
        string NumberFormat { get; set; }

        /// <summary>
        /// Indicates whether usage reporting is approved or not.
        /// </summary>
        bool IsUsageReportingApproved { get; set; }

        /// <summary>
        /// Indicates whether analytics reporting is approved or not.
        /// </summary>
        bool IsAnalyticsReportingApproved { get; set; }

        /// <summary>
        /// Indicates first run
        /// </summary>
        bool IsFirstRun { get; set; }

        /// <summary>
        /// Returns the last X coordinate of the Dynamo window.
        /// </summary>
        double WindowX { get; set; }

        /// <summary>
        /// Returns the last Y coordinate of the Dynamo window.
        /// </summary>
        double WindowY { get; set; }

        /// <summary>
        /// Returns the last height of the Dynamo window.
        /// </summary>
        double WindowH { get; set; }

        /// <summary>
        /// Returns the last width of the Dynamo window.
        /// </summary>
        double WindowW { get; set; }

        /// <summary>
        /// Returns maximal count of recent files which can be displayed
        /// </summary>
        int MaxNumRecentFiles { get; set; }

        /// <summary>
        /// Returns list of recent files
        /// </summary>
        List<string> RecentFiles { get; set; }

        /// <summary>
        /// Returns list of backup files
        /// </summary>
        List<string> BackupFiles { get; set; }

        /// <summary>
        /// Returns list of packages used by the Package Manager to determine
        /// which packages are marked for deletion.
        /// </summary>
        List<string> PackageDirectoriesToUninstall { get; set; }

        /// <summary>
        /// Returns list of folders containing zero-touch nodes and custom nodes.
        /// </summary>
        List<string> CustomPackageFolders { get; set; }

        /// <summary>
        /// Indicates whether surface and solid edges will 
        /// be rendered.
        /// </summary>
        bool ShowEdges { get; set; }

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
