using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Search.SearchElements;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    internal class SingleResultItem : ClusterResultItem
    {
        private void InitTopology(int port)
        {
            EntryNodeIndex = 0;
            EntryNodeInPort = port;
            EntryNodeOutPort = port;
            Topology = new TopologyItem
            {
                Nodes = new List<NodeItem> { new NodeItem {
                    Id = Guid.NewGuid().ToString(),
                    Type = new NodeType { Id = CreationName } } },
                Connections = new List<ConnectionItem>()
            };
        }
        public SingleResultItem(NodeSearchElement model, double score = 1.0)
        {
            Assembly = model.Assembly;
            IconName = model.IconName;
            Description = model.Name;
            Parameters = model.Parameters;
            CreationName = model.CreationName;
            Probability = score;
            Title = Description;
            InitTopology(model.AutoCompletionNodeElementInfo.PortToConnect);
        }

        public SingleResultItem(NodeModel nodeModel, int portToConnect, double score = 1.0)
        {
            Assembly = nodeModel.GetType().Assembly.GetName().Name;
            IconName = nodeModel.GetType().Name;
            Description = nodeModel.Name;
            Parameters = $"({string.Join(", ", nodeModel.InPorts.Select(x => x.Name))})";
            CreationName = nodeModel.CreationName;
            Probability = score;
            Title = Description;
            InitTopology(portToConnect);
        }

        internal string Assembly { get; set; }

        internal string IconName { get; set; }

        internal string Parameters { get; set; }

        internal string CreationName { get; set; }
    }
}
