using Greg.Responses;

namespace DynamoAnalyzer.Models
{
    /// <summary>
    /// PackageHeader extension to provide the Index property
    /// </summary>
    public class PackageHeaderCustom : PackageHeader
    {
        public int Index { get; set; }
    }
}
