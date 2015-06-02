using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;

using DynamoUtilities;

namespace Dynamo
{
    /// <summary>
    /// PreferenceSettings is a class for GUI to persist certain settings.
    /// Upon running of the GUI, those settings that are persistent will be loaded
    /// from a XML file from DYNAMO_SETTINGS_FILE.
    /// When GUI is closed, the settings into the XML file.
    /// </summary>
    public class PreferenceSettings : NotificationObject, IPreferences
    {
        public const int DefaultMaxNumRecentFiles = 10;
        public static string DynamoTestPath = null;
        private string numberFormat;
        private string lastUpdateDownloadPath;
        private int maxNumRecentFiles;
        
        // Variables of the settings that will be persistent

        #region Collect Information Settings
        public bool IsFirstRun { get; set; }
        public bool IsUsageReportingApproved { get; set; }
        public bool IsAnalyticsReportingApproved { get; set; }
        #endregion

        /// <summary>
        /// The width of the library pane.
        /// </summary>
        public int LibraryWidth { get; set; }

        /// <summary>
        /// The height of the console display.
        /// </summary>
        public int ConsoleHeight { get; set; }

        /// <summary>
        /// Should connectors be visible?
        /// </summary>
        public bool ShowConnector { get; set; }

        /// <summary>
        /// The types of connector: Bezier or Polyline.
        /// </summary>
        public ConnectorType ConnectorType { get; set; }

        /// <summary>
        /// Should the background 3D preview be shown?
        /// </summary>
        public bool FullscreenWatchShowing { get; set; }

        /// <summary>
        /// The decimal precision used to display numbers.
        /// </summary>
        public string NumberFormat
        {
            get { return numberFormat; }
            set
            {
                numberFormat = value;
                RaisePropertyChanged("NumberFormat");
            }
        }

        /// <summary>
        /// The maximum number of recent file paths to be saved.
        /// </summary>
        public int MaxNumRecentFiles
        {
            get { return maxNumRecentFiles; }
            set
            {
                if (value > 0)
                {
                    maxNumRecentFiles = value;
                }
                else
                {
                    maxNumRecentFiles = DefaultMaxNumRecentFiles;
                }
                RaisePropertyChanged("MaxNumRecentFiles");
            }
        }

        /// <summary>
        /// A list of recently opened file paths.
        /// </summary>
        public List<string> RecentFiles { get; set; }

        /// <summary>
        /// A list of backup file paths.
        /// </summary>
        public List<string> BackupFiles { get; set; }

        /// <summary>
        /// A list of packages used by the Package Manager to determine
        /// which packages are marked for deletion.
        /// </summary>
        public List<string> PackageDirectoriesToUninstall { get; set; }

        /// <summary>
        /// The last X coordinate of the Dynamo window.
        /// </summary>
        public double WindowX { get; set; }

        /// <summary>
        /// The last Y coordinate of the Dynamo window.
        /// </summary>
        public double WindowY { get; set; }

        /// <summary>
        /// The last width of the Dynamo window.
        /// </summary>
        public double WindowW { get; set; }

        /// <summary>
        /// The last height of the Dynamo window.
        /// </summary>
        public double WindowH { get; set; }

        /// <summary>
        /// Should Dynamo use hardware acceleration if it is supported?
        /// </summary>
        public bool UseHardwareAcceleration { get; set; }

        /// <summary>
        /// This defines how long (in milliseconds) will the graph be automatically saved.
        /// </summary>
        public int BackupInterval { get; set; }

        /// <summary>
        /// This defines how many files will be backed up.
        /// </summary>
        public int BackupFilesCount { get; set; }

        /// <summary>
        /// Indicates if the user has accepted the terms of 
        /// use for downloading packages from package manager.
        /// </summary>
        public bool PackageDownloadTouAccepted { get; set; }

        /// <summary>
        /// Indicates the default state of the "Open in Manual Mode"
        /// checkbox in OpenFileDialog
        /// </summary>
        public bool OpenFileInManualExecutionMode { get; set; }

        public PreferenceSettings()
        {
            RecentFiles = new List<string>();
            WindowH = 768;
            WindowW = 1024;
            WindowY = 0.0;
            WindowX = 0.0;

            // Default Settings
            IsFirstRun = true;
            IsUsageReportingApproved = false;
            LibraryWidth = 304;
            ConsoleHeight = 0;
            ShowConnector = true;
            ConnectorType = ConnectorType.BEZIER;
            FullscreenWatchShowing = true;
            PackageDirectoriesToUninstall = new List<string>();
            NumberFormat = "f3";
            UseHardwareAcceleration = true;
            PackageDownloadTouAccepted = false;
            maxNumRecentFiles = DefaultMaxNumRecentFiles;
            OpenFileInManualExecutionMode = false;

            BackupInterval = 60000; // 1 minute
            BackupFilesCount = 1;
            BackupFiles = new List<string>();
        }

        /// <summary>
        /// Save PreferenceSettings in XML File Path if possible,
        /// else return false
        /// </summary>
        /// <param name="filePath">Path of the XML File</param>
        /// <returns>Whether file is saved or error occurred.</returns>
        public bool Save(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(PreferenceSettings));
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, this);
                    fs.Close(); // Release file lock
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// Save PreferenceSettings in a default directory when no path is 
        /// specified.
        /// </summary>
        /// <param name="preferenceFilePath">The file path to save preference
        /// settings to. If this parameter is null or empty string, preference 
        /// settings will be saved to the default path.</param>
        /// <returns>Whether file is saved or error occurred.</returns>
        public bool SaveInternal(string preferenceFilePath)
        {
            if (!string.IsNullOrEmpty(DynamoTestPath))
            {
                preferenceFilePath = DynamoTestPath;
            }

            return Save(preferenceFilePath);
        }

        /// <summary>
        /// Return PreferenceSettings from XML path if possible,
        /// else return PreferenceSettings with default values
        /// </summary>
        /// <param name="filePath">Path of the XML File</param>
        /// <returns>
        /// Stored PreferenceSettings from xml file or
        /// Default PreferenceSettings if xml file is not found.
        /// </returns>
        public static PreferenceSettings Load(string filePath)
        {
            var settings = new PreferenceSettings();

            if (string.IsNullOrEmpty(filePath) || (!File.Exists(filePath)))
                return settings;

            try
            {
                var serializer = new XmlSerializer(typeof(PreferenceSettings));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    settings = serializer.Deserialize(fs) as PreferenceSettings;
                    fs.Close(); // Release file lock
                }
            }
            catch (Exception) { }

            return settings;
        }
    }
}