namespace DynamoAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class Result
    {
        public string RuleId { get; set; }
        public string Level { get; set; }
        public Message Message { get; set; }
        public Location[] Locations { get; set; }
    }
}
