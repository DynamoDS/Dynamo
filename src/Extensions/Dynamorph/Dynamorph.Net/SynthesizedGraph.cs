using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamorph
{
    class Node
    {
        public Node(string identifier, string name)
        {
            this.Identifier = identifier;
            this.Name = name;
        }

        public string Identifier { get; private set; }
        public string Name { get; private set; }
    }

    class Edge
    {
        public Edge(string startNodeId, string endNodeId)
        {
            this.StartNodeId = startNodeId;
            this.EndNodeId = EndNodeId;
        }

        public string StartNodeId { get; private set; }
        public string EndNodeId { get; private set; }
    }

    public class SynthesizedGraph
    {
        private List<Node> nodes = new List<Node>();
        private List<Edge> edges = new List<Edge>();

        public void AddNode(string identifier, string name)
        {
            // Add the node into the list when one does not already exist.
            if (nodes.Where(n => n.Identifier == identifier).Any() == false)
                nodes.Add(new Node(identifier, name));
        }

        public void AddEdge(string startNodeId, string endNodeId)
        {
            var matchingEdge = edges.Where((x) =>
            {
                if (x.StartNodeId != startNodeId)
                    return false;

                return x.EndNodeId == endNodeId;
            });

            // Only add the edge when one does not exist.
            if (matchingEdge.Any() == false)
                edges.Add(new Edge(startNodeId, endNodeId));
        }

        internal IEnumerable<Node> Nodes { get { return this.nodes; } }
        internal IEnumerable<Edge> Edges { get { return this.edges; } }
    }
}
