using System;
using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Properties;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Represents the stringified version of the nodes connections from a graph
    /// </summary>
    public class GraphChecksumItem
    {
        public string GraphId { get; set; }

        public string Checksum { get; set; }   
    }
}
