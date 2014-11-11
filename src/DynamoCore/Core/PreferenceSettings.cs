using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using DynamoUnits;

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
        public static string DYNAMO_TEST_PATH = null;
        const string DYNAMO_SETTINGS_FILE = "DynamoSettings.xml";
        private LengthUnit _lengthUnit;
        private AreaUnit _areaUnit;
        private VolumeUnit _volumeUnit;
        private string _numberFormat;
        private string lastUpdateDownloadPath;

        // Variables of the settings that will be persistent

        #region Collect Information Settings
        public bool IsFirstRun { get; set; }
        public bool IsUsageReportingApproved { get; set; }
        public bool IsAnalyticsReportingApproved { get; set; }
        #endregion

        public int ConsoleHeight { get; set; }
        public bool ShowConnector { get; set; }
        public ConnectorType ConnectorType { get; set; }
        public bool FullscreenWatchShowing { get; set; }

        public string NumberFormat
        {
            get { return _numberFormat; }
            set
            {
                _numberFormat = value;
                RaisePropertyChanged("NumberFormat");
            }
        }

        public LengthUnit LengthUnit
        {
            get { return _lengthUnit; }
            set
            {
                _lengthUnit = value;
                RaisePropertyChanged("LengthUnit");
            }
        }

        public int MaxNumRecentFiles
        {
            get { return 10; }
            set { }
        }

        private List<string> _recentFiles = new List<string>();
        public List<string> RecentFiles
        {
            get { return _recentFiles; }
            set { _recentFiles = value; }
        }

        public List<string> PackageDirectoriesToUninstall { get; set; }

        public AreaUnit AreaUnit
        {
            get { return _areaUnit; }
            set
            {
                _areaUnit = value;
                RaisePropertyChanged("AreaUnit");
            }
        }

        public VolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;
                RaisePropertyChanged("VolumeUnit");
            }
        }

        public double WindowX { get; set; }
        public double WindowY { get; set; }
        public double WindowW { get; set; }
        public double WindowH { get; set; }

        public string LastUpdateDownloadPath
        {
            get { return lastUpdateDownloadPath; }
            set
            {
                if (!File.Exists(value))
                {
                    lastUpdateDownloadPath = "";
                }
                else
                {
                    lastUpdateDownloadPath = value; 
                }
            }
        }

        public PreferenceSettings()
        {
            WindowH = 768;
            WindowW = 1024;
            WindowY = 0.0;
            WindowX = 0.0;

            // Default Settings
            IsFirstRun = true;
            IsUsageReportingApproved = false;
            ConsoleHeight = 0;
            ShowConnector = true;
            ConnectorType = ConnectorType.BEZIER;
            FullscreenWatchShowing = true;
            LengthUnit = LengthUnit.Meter;
            AreaUnit = DynamoUnits.AreaUnit.SquareMeter;
            VolumeUnit = VolumeUnit.CubicMeter;
            PackageDirectoriesToUninstall = new List<string>();
            NumberFormat = "f3";
            LastUpdateDownloadPath = "";
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
                var serializer = new XmlSerializer(typeof (PreferenceSettings));
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
        /// Save PreferenceSettings in a default directory when no path is specified
        /// </summary>
        /// <returns>Whether file is saved or error occurred.</returns>
        public bool Save()
        {
            if ( DYNAMO_TEST_PATH == null )
                // Save in User Directory Path
                return Save(GetSettingsFilePath());
            else
                // Support Testing
                return Save(DYNAMO_TEST_PATH);
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
        
        /// <summary>
        /// Return PreferenceSettings from Default XML path
        /// </summary>
        /// <returns>
        /// Stored PreferenceSettings from default xml file or
        /// Default PreferenceSettings if default xml file is not found.
        /// </returns>
        public static PreferenceSettings Load()
        {
            if ( DYNAMO_TEST_PATH == null )
                // Save in User Directory Path
                return Load(GetSettingsFilePath());
            else
                // Support Testing
                return Load(DYNAMO_TEST_PATH);
        }

        /// <summary>
        /// Return PreferenceSettings Default XML File Path if possible
        /// </summary>
        public static string GetSettingsFilePath()
        {
            try
            {
                return (Path.Combine(DynamoPathManager.Instance.AppData, DYNAMO_SETTINGS_FILE));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}