namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    internal class Run
    {
        internal Tool Tool { get; set; }
        internal Result[] Results { get; set; }
        internal string ColumnKind { get; set; }
    }
}
