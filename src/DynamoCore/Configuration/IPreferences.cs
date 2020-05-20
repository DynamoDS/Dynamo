using System;
using System.Collections.Generic;
using Dynamo.Graph.Connectors;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// An interface which defines preference settings.
    /// </summary>
    public interface IPreferences
    {
        /// <summary>
        /// Collection of pairs [BackgroundPreviewName;isActive]
        /// </summary>
        List<BackgroundPreviewActiveState> BackgroundPreviews { get; set; }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        bool ShowPreviewBubbles { get; set; }

        /// <summary>
        /// Returns height of console
        /// </summary>
        int ConsoleHeight { get; set; }

        /// <summary>
        /// Indicates whether connector should be displayed on canvas or not.
        /// </summary>
        bool ShowConnector { get; set; }

        /// <summary>
        /// Indicates which type of connector's should be displayed on canvas.
        /// I.e. bezier or polyline
        /// </summary>
        ConnectorType ConnectorType { get; set; }

        /// <summary>
        /// Indicates whether background grid is visible or not.
        /// </summary>
        bool IsBackgroundGridVisible { get; set; }

        /// <summary>
        /// Indicates whether background preview is active or not.
        /// </summary>
        [Obsolete("Property will be deprecated in Dynamo 3.0, please use BackgroundPreviews")]
        bool IsBackgroundPreviewActive { get; set; }

        /// <summary>
        /// Returns the decimal precision used to display numbers.
        /// </summary>
        string NumberFormat { get; set; }

        /// <summary>
        /// Indicates whether usage reporting is approved or not.
        /// </summary>
        [Obsolete("Property will be deprecated in Dynamo 3.0")]
        bool IsUsageReportingApproved { get; set; }

        /// <summary>
        /// Indicates whether Google analytics reporting is approved or not.
        /// </summary>
        bool IsAnalyticsReportingApproved { get; set; }

        /// <summary>
        /// Indicates whether ADP analytics reporting is approved or not.
        /// </summary>
        bool IsADPAnalyticsReportingApproved { get; set; }

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

        /// <summary>
        /// Return full path to the Python (.py) file to use as a starting template when creating a new PythonScript Node.
        /// </summary>
        string PythonTemplateFilePath { get; set; }

        /// <summary>
        /// Returns active state of specified background preview 
        /// </summary>
        /// <param name="name">Background preview name</param>
        /// <returns>The active state</returns>
        bool GetIsBackgroundPreviewActive(string name);

        /// <summary>
        /// Sets active state of specified background preview 
        /// </summary>
        /// <param name="name">Background preview name</param>
        /// <param name="value">Active state to set</param>
        void SetIsBackgroundPreviewActive(string name, bool value);
    }

    /// <summary>
    /// Temporary interface to avoid breaking changes.
    /// TODO: Merge with IPreferences for 3.0 (DYN-1699)
    /// </summary>
    public interface IRenderPrecisionPreference
    {
        ///<summary>
        ///Indicate which render precision will be used
        ///</summary>
        int RenderPrecision { get; set; }
    }

    /// <summary>
    /// Represents data about active state of preview background
    /// </summary>
    public class BackgroundPreviewActiveState
    {
        /// <summary>
        /// Name of background preview 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flag which indicates if background preview is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of <c name="BackgroundPreviewActiveState"/> class
        /// </summary>
        public BackgroundPreviewActiveState()
        {
            // Default value
            IsActive = true;
        }
    }
}
