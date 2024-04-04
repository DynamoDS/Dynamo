using Newtonsoft.Json;

namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    internal class Sarif
    {
        [JsonProperty("$schema")]
        internal string Schema { get; set; }
        internal string Version { get; set; }
        internal Run[] Runs { get; set; }
    }
}
