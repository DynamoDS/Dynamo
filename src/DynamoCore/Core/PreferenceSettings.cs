using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Dynamo.Models;

namespace Dynamo
{
    /// <summary>
    /// PreferenceSettings is a class for GUI to persist certain settings.
    /// Upon running of the GUI, those settings that are persistent will be loaded
    /// from a XML file from DYNAMO_SETTINGS_FILE.
    /// When GUI is closed, the settings into the XML file.
    /// </summary>
    public class PreferenceSettings
    {
        public static string DYNAMO_TEST_PATH = null;
        const string DYNAMO_SETTINGS_DIRECTORY = @"Autodesk\Dynamo\";
        const string DYNAMO_SETTINGS_FILE = "DynamoSettings.xml";

        // Variables of the settings that will be persistent
        public bool ShowConsole { get; set; }
        public bool ShowConnector { get; set; }
        public ConnectorType ConnectorType { get; set; }
        public bool FullscreenWatchShowing { get; set; }

        public PreferenceSettings()
        {
            // Default Settings
            this.ShowConsole = false;
            this.ShowConnector = true;
            this.ConnectorType = ConnectorType.BEZIER;
            this.FullscreenWatchShowing = true;
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
                XmlSerializer serializer = new XmlSerializer(typeof(PreferenceSettings));
                FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fs, this);
                fs.Close(); // Release file lock
                return true;
            }
            catch (Exception) { }
            
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
            PreferenceSettings settings = new PreferenceSettings();
            
            if (string.IsNullOrEmpty(filePath) || (!File.Exists(filePath)))
                return settings;
            
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PreferenceSettings));
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                settings = serializer.Deserialize(fs) as PreferenceSettings;
                fs.Close(); // Release file lock
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
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                appDataFolder = Path.Combine(appDataFolder, DYNAMO_SETTINGS_DIRECTORY);
                
                if (Directory.Exists(appDataFolder) == false)
                    Directory.CreateDirectory(appDataFolder);
                
                return (Path.Combine(appDataFolder, DYNAMO_SETTINGS_FILE));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}