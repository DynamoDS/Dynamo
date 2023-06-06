namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class Run
    {
        public Tool Tool { get; set; }
        public Result[] Results { get; set; }
        public string ColumnKind { get; set; }
    }
}
