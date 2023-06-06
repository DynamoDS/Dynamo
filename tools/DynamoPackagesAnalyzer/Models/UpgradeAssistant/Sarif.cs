using Newtonsoft.Json;

namespace DynamoPackagesAnalyzer.Models.UpgradeAssistant
{
    /// <summary>
    /// DTO used in the sarif file
    /// </summary>
    public class Sarif
    {
        [JsonProperty("$schema")]
        public string Schema { get; set; }
        public string Version { get; set; }
        public Run[] Runs { get; set; }
    }
}
