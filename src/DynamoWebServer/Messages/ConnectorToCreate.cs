using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;

namespace DynamoWebServer.Messages
{
    /// <summary>
    /// This class represents the data that is required to create connectors on the 
    /// client. When a file is opened on Dynamo Server, this information is delivered 
    /// to the client to generate connectors found in the file. The data contains 
    /// only outgoing connectors since incoming connectors were added as outgoing 
    /// connectors from other upstream nodes.
    /// </summary>
    public class ConnectorToCreate
    {
        /// <summary>
        /// Guid of the node's output port
        /// </summary>
        [DataMember]
        public string StartNodeId { get; private set; }

        /// <summary>
        /// Index of the output port
        /// </summary>
        [DataMember]
        public int StartPortIndex { get; private set; }

        /// <summary>
        /// Guid of the node's input port
        /// </summary>
        [DataMember]
        public string EndNodeId { get; private set; }

        /// <summary>
        /// Index of the output port
        /// </summary>
        [DataMember]
        public int EndPortIndex { get; private set; }

        public ConnectorToCreate(string sNodeId, int sIndex, string eNodeId, int eIndex)
        {
            StartNodeId = sNodeId;
            StartPortIndex = sIndex;
            EndNodeId = eNodeId;
            EndPortIndex = eIndex;
        }

        public static IEnumerable<ConnectorToCreate> GetOutgoingConnectors(NodeModel node)
        {
            var result = new List<ConnectorToCreate>();
            foreach (var outPort in node.OutPorts)
            {
                foreach (var connector in outPort.Connectors)
                {
                    string startId = connector.Start.Owner.GUID.ToString();
                    int startIndex = connector.Start.Index;

                    string endId = connector.End.Owner.GUID.ToString();
                    int endIndex = connector.End.Index;

                    result.Add(new ConnectorToCreate(startId, startIndex, endId, endIndex));
                }
            }

            return result;
        }
    }
}
