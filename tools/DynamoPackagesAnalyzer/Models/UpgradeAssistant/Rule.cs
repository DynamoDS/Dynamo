namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    internal class Rule
    {
        internal string Id { get; set; }
        internal string Name { get; set; }
        internal RuleDescription FullDescription { get; set; }
    }
}
