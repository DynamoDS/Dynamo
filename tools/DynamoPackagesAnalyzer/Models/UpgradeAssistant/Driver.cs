namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    internal class Driver
    {
        internal string Name { get; set; }
        internal string SemanticVersion { get; set; }
        internal string InformationUri { get; set; }
        internal Rule[] Rules { get; set; }
    }
}
