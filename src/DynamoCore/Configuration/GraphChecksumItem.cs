using System;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Represents the stringified version of the nodes connections from a graph
    /// </summary>
    [Obsolete("This property is not needed anymore in the preference settings and can be removed in a future version of Dynamo.")]
    public class GraphChecksumItem
    {
        public string GraphId { get; set; }

        public string Checksum { get; set; }   
    }
}
