using Dynamo.Models;
using Dynamo.Units;

namespace Dynamo.Interfaces
{
    public interface IPreferences
    {
        bool ShowConsole { get; set; }
        bool ShowConnector { get; set; }
        ConnectorType ConnectorType { get; set; }
        bool FullscreenWatchShowing { get; set; }
        string NumberFormat { get; set; }
        DynamoLengthUnit LengthUnit { get; set; }
        DynamoAreaUnit AreaUnit { get; set; }
        DynamoVolumeUnit VolumeUnit { get; set; }
        bool IsUsageReportingApproved { get; set; }
        bool IsFirstRun { get; set; }

        /// <summary>
        /// Save PreferenceSettings in XML File Path if possible,
        /// else return false
        /// </summary>
        /// <param name="filePath">Path of the XML File</param>
        /// <returns>Whether file is saved or error occurred.</returns>
        bool Save(string filePath);

        /// <summary>
        /// Save PreferenceSettings in a default directory when no path is specified
        /// </summary>
        /// <returns>Whether file is saved or error occurred.</returns>
        bool Save();
    }
}
