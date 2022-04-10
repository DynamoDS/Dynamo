using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Graph.Nodes;

namespace Dynamo.GraphNodeManager.ViewModels
{
    /// <summary>
    /// The ViewModel class to represent a Dynamo Node 
    /// </summary>
    public class NodeViewModel : NotificationObject
    {
        private string name = String.Empty;

        /// <summary>
        /// Node Name
        /// </summary>
        public string Name
        {
            get
            {
                name = NodeModel?.Name;
                return name;
            }
            internal set { name = value; }
        }

        internal NodeModel NodeModel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        public NodeViewModel(NodeModel node)
        {
            NodeModel = node;
        }
    }
}
