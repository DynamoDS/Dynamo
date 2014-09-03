using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;

namespace DynamoWebServer.Messages
{
    /// <summary>
    /// The class that represents data for creating connectors
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

        public ConnectorToCreate(string sNodeID, int sIndex, string eNodeID, int eIndex)
        {
            StartNodeId = sNodeID;
            StartPortIndex = sIndex;
            EndNodeId = eNodeID;
            EndPortIndex = eIndex;
        }

        public static List<ConnectorToCreate> GetComingOutConnectors(NodeModel node)
        {
            var result = new List<ConnectorToCreate>();
            int startIndex, endIndex;
            string startID, endID;
            foreach (var outPort in node.OutPorts)
            {
                startIndex = outPort.Index;
                foreach (var connector in outPort.Connectors)
                {
                    startID = connector.Start.Owner.GUID.ToString();
                    startIndex = connector.Start.Index;

                    endID = connector.End.Owner.GUID.ToString();
                    endIndex = connector.End.Index;

                    result.Add(new ConnectorToCreate(startID, startIndex, endID, endIndex));
                }
            }
            return result;
        }
    }
}
