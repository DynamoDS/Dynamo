namespace DynamoAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class Rule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RuleDescription FullDescription { get; set; }
    }
}
