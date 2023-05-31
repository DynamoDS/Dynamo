namespace DynamoAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class Driver
    {
        public string Name { get; set; }
        public string SemanticVersion { get; set; }
        public string InformationUri { get; set; }
        public Rule[] Rules { get; set; }

    }
}
