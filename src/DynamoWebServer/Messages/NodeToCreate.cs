using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;

namespace DynamoWebServer.Messages
{
    public class NodeToCreate
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string _id { get; private set; }

        /// <summary>
        /// CreatingName of the specified node
        /// </summary>
        [DataMember]
        public string CreatingName { get; private set; }

        /// <summary>
        /// CreatingName of the specified node
        /// </summary>
        [DataMember]
        public string DisplayedName { get; private set; }

        /// <summary>
        /// X and Y coordinate of the specified node
        /// </summary>
        [DataMember]
        public IEnumerable<double> Position { get; private set; }

        /// <summary>
        /// Value of the specified node if it's number node, code block node or custom node
        /// </summary>
        [DataMember]
        public object Value { get; private set; }

        /// <summary>
        /// Indicates whether the node is an Custom Node instance 
        /// </summary>
        [DataMember]
        public bool IsCustomNode { get; private set; }

        /// <summary>
        /// Call this method when this node should be created on a client.
        /// </summary>
        /// <param name="node">The specified node</param>
        public NodeToCreate(NodeModel node, string data)
        {
            this._id = node.GUID.ToString();
            this.CreatingName = node.CreatingName;
            if (CreatingName == "Number")
            {
                double number;
                Value = double.TryParse(data, out number) ? number : 0;
            }
            else if (node is CodeBlockNodeModel || node is Function)
            {
                Value = data;
                IsCustomNode = node is Function;
            }
                    
            this.DisplayedName = node.NickName;
            this.Position = new List<double> { node.X, node.Y };
        }
    }
}
