using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamorph
{
    public interface ISynthesizedGraph
    {
        void AddNode(string identifier, string name);
        void AddEdge(string startNodeId, string endNodeId);
    }

    internal class Node
    {
        internal enum MarkStatus { None, Temporary, Permanent }

        internal Node(string identifier, string name)
        {
            this.Identifier = identifier;
            this.Name = name;
            this.Marking = MarkStatus.None;
        }

        internal int Depth { get; set; }
        internal string Identifier { get; private set; }
        internal string Name { get; private set; }
        internal MarkStatus Marking { get; set; }
    }

    internal class Edge
    {
        internal Edge(string startNodeId, string endNodeId)
        {
            this.StartNodeId = startNodeId;
            this.EndNodeId = endNodeId;
        }

        internal string StartNodeId { get; private set; }
        internal string EndNodeId { get; private set; }
    }

    internal class SynthesizedGraph : ISynthesizedGraph
    {
        private List<Node> nodes = new List<Node>();
        private List<Edge> edges = new List<Edge>();

        #region Interface Method Implementations

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

        #endregion

        #region Internal Class Operational Methods

        public void BuildGraphStructure()
        {
            LabelGraphNodes(); // Label the entire graph with depth info.

            // Sort the depth value from small to large.
            this.nodes.Sort((n1, n2) => n1.Depth - n2.Depth);
        }

        #endregion

        #region Class Properties

        internal IEnumerable<Node> Nodes { get { return this.nodes; } }
        internal IEnumerable<Edge> Edges { get { return this.edges; } }

        #endregion

        #region Private Class Helper Methods

        private void LabelGraphNodes()
        {
            // Reset the depth value of each node to minimal value.
            this.nodes.ForEach(n => n.Depth = int.MinValue);

            // Get a list of all root nodes.
            var rootNodes = nodes.Where(n =>
            {
                // A root node is a node which doesn't have input edge.
                return !(edges.Where(e => e.EndNodeId == n.Identifier).Any());
            });

            foreach (var rootNode in rootNodes)
                LabelGraphNode(rootNode, 0);
        }

        private void LabelGraphNode(Node node, int depth)
        {
            // The node is already marked with a larger depth value, which 
            // means this node was visited before this call with much longer
            // chain of ancestor list. No point going further to label them.
            // 
            if (node.Depth > depth)
                return;

            node.Depth = depth;
            var childNodes = GetChildNodes(node);
            foreach (var childNode in childNodes)
                LabelGraphNode(childNode, depth + 1);
        }

        private void TopologicalSort()
        {
            // Reset the marking status of each node to None.
            this.nodes.ForEach(n => n.Marking = Node.MarkStatus.None);
            List<Node> sortedNodes = new List<Node>();

            while (true)
            {
                var unvisitedQuery = this.nodes.Where((n) =>
                {
                    return n.Marking != Node.MarkStatus.Permanent;
                });

                if (unvisitedQuery.Any() == false)
                    break; // No more unvisited node.

                VisitNode(unvisitedQuery.ElementAt(0), sortedNodes);
            }

            this.nodes.Clear();
            this.nodes.AddRange(sortedNodes);
        }

        private void VisitNode(Node node, List<Node> sortedNodes)
        {
            if (node.Marking != Node.MarkStatus.None)
            {
                var message = "Cyclic graph detected!";
                throw new InvalidOperationException(message);
            }

            node.Marking = Node.MarkStatus.Temporary;
            foreach (var childNode in GetChildNodes(node))
                VisitNode(childNode, sortedNodes);

            node.Marking = Node.MarkStatus.Permanent;
            sortedNodes.Insert(0, node);
        }

        private IEnumerable<Node> GetChildNodes(Node node)
        {
            var endNodeQuery = this.edges.Where((e) =>
            {
                // Get edges connecting from this "node".
                return e.StartNodeId == node.Identifier;

            }).Select(e => e.EndNodeId);

            var nodeQuery = this.nodes.Where((n) =>
            {
                // A node will be considered a child node 
                // if it is at the end of connecting edges.
                return endNodeQuery.Contains(n.Identifier);
            });

            return new List<Node>(nodeQuery);
        }

        #endregion
    }
}
