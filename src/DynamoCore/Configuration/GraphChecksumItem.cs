using System;
using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Properties;

namespace Dynamo.Configuration
{
    /// <summary>
    /// Represents the stringified version of the nodes connections from a graph
    /// </summary>
    public class GraphChecksumItem : NotificationObject
    {
        private string graphId;
        private string checksum;

        public string GraphId
        {
            get { return graphId; }
            set
            {
                graphId = value;
                RaisePropertyChanged(nameof(GraphId));
            }
        }

        public string Checksum
        {
            get { return checksum; }
            set
            {
                checksum = value;
                RaisePropertyChanged(nameof(Checksum));
            }
        }
    }
}
