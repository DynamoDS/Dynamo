using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;

namespace DynamoWebServer.Messages
{
    /// <summary>
    /// This class represents the data that is required to recreate nodes on the 
    /// client. When a file is uploaded and opened on Dynamo Server, this 
    /// information is delivered to the client to generate nodes found in the file.
    /// </summary>
    public class NodeToCreate
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string _id { get; private set; }

        /// <summary>
        /// CreationName of the specified node
        /// </summary>
        [DataMember]
        public string CreationName { get; private set; }

        /// <summary>
        /// CreationName of the specified node
        /// </summary>
        [DataMember]
        public string DisplayName { get; private set; }

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
            this.CreationName = GetCreationName(node);
            if (CreationName == "Number")
            {
                double number;
                Value = double.TryParse(data, out number) ? number : 0;
            }
            else if (CreationName == "Boolean")
            {
                bool boolValue;
                bool.TryParse(data, out boolValue);
                Value = boolValue;
            }
            else if (node is CodeBlockNodeModel || node is Function)
            {
                Value = data;
                IsCustomNode = node is Function;
            }
                    
            this.DisplayName = node.NickName;
            this.Position = new List<double> { node.X, node.Y };
        }

        private string GetCreationName(NodeModel node)
        {
            if (!string.IsNullOrEmpty(node.CreationName))
                return node.CreationName;

            Type type = node.GetType();
            object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
            if (attribs.Length > 0)
            {
                var elCatAttrib = attribs[0] as NodeNameAttribute;
                return elCatAttrib.Name;
            }

            return type.Name;
        }
    }
}
