namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    internal class Result
    {
        internal string RuleId { get; set; }
        internal string Level { get; set; }
        internal Message Message { get; set; }
        internal Location[] Locations { get; set; }
    }
}
