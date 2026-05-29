using Newtonsoft.Json;
using System;

namespace Dynamo.Graph.Workspaces.Locking
{
    internal sealed class GraphLockInfo         // DO WE NEED ALL THOSE?
    {
        [JsonProperty("schemaVersion")]
        internal int SchemaVersion { get; set; }

        [JsonProperty("sessionId")]
        internal Guid SessionId { get; set; }

        [JsonProperty("graphPath")]
        internal string GraphPath { get; set; }

        [JsonProperty("userName")]
        internal string UserName { get; set; }

        [JsonProperty("machinename")]
        internal string MachineName { get; set; }

        [JsonProperty("processid")]
        internal int ProcessId { get; set; }

        [JsonProperty("processStartUtc")]
        internal DateTime ProcessStartUtc { get; set; }

        [JsonProperty("dynamoVersion")]
        internal string DynamoVersion { get; set; }

        [JsonProperty("acquiredUtc")]
        internal DateTime AcquiredUtc { get; set; }

        [JsonProperty("lastHeartbeatUtc")]
        internal DateTime LastHeartbeatUtc { get; set; }
    }
}
