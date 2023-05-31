namespace DynamoAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class PhysicalLocation
    {
        public ArtifactLocation ArtifactLocation { get; set; }
        public Region Region { get; set; }
    }
}
