using System;
using System.Collections.Generic;
using Dynamo.Search.SearchElements;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    internal class SingleResultItem : ClusterResultItem
    {

        public SingleResultItem(NodeSearchElement model, double score = 1.0)
        {
            Assembly = model.Assembly;
            IconName = model.IconName;
            Description = model.Name;
            Parameters = model.Parameters;
            CreationName = model.CreationName;
            Probability = score;
            Title = Description;
            EntryNodeIndex = 0;
            EntryNodeInPort = model.AutoCompletionNodeElementInfo.PortToConnect;
            EntryNodeOutPort = model.AutoCompletionNodeElementInfo.PortToConnect;
            Topology = new TopologyItem
            {
                Nodes = new List<NodeItem> { new NodeItem {
                    Id = Guid.NewGuid().ToString(),
                    Type = new NodeType { Id = CreationName } } },
                Connections = new List<ConnectionItem>()
            };
        }

        internal string Assembly { get; set; }

        internal string IconName { get; set; }

        internal string Parameters { get; set; }

        internal string CreationName { get; set; }
    }
}
