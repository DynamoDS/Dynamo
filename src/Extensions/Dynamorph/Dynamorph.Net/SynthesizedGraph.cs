using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dynamorph
{
    struct Config
    {
        internal static readonly double HorzGap = 32.0;
        internal static readonly double VertGap = 16.0;
        internal static readonly double NodeWidth = 128.0;
        internal static readonly double NodeHeight = 32.0;
        internal static readonly double HorzSpace = ((2 * HorzGap) + NodeWidth);
        internal static readonly double VertSpace = ((2 * VertGap) + NodeHeight);
    }

    public interface ISynthesizedGraph
    {
        void AddNode(string identifier, string name);
        void AddEdge(string startNodeId, string endNodeId);
    }

    internal class Node
    {
        private Rect rect = new Rect();
        private Point inputPoint = new Point();
        private Point outputPoint = new Point();

        internal Node(string identifier, string name)
        {
            this.Identifier = identifier;
            this.Name = name;
            this.ChildrenCount = -1;
        }

        internal void UpdateNodeLayout()
        {
            var top = (Config.VertGap + (Config.VertSpace * DisplayRow));
            var left = (Config.HorzGap + (Config.HorzSpace * Depth));
            this.rect = new Rect(left, top, Config.NodeWidth, Config.NodeHeight);

            inputPoint = new Point(rect.Left, rect.Top + (0.5 * rect.Height));
            outputPoint = new Point(rect.Right, inputPoint.Y);
        }

        #region Public Class Properties

        internal int Depth { get; set; }
        internal int DisplayRow { get; set; }
        internal int ChildrenCount { get; set; }
        internal string Identifier { get; private set; }
        internal string Name { get; private set; }
        internal Rect Rect { get { return this.rect; } }
        internal Point InputPoint { get { return this.inputPoint; } }
        internal Point OutputPoint { get { return this.outputPoint; } }

        #endregion
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

        internal void BuildGraphStructure()
        {
            LabelGraphNodes(); // Label the entire graph with depth info.
            CalculateChildNode(); // Compute number of children for each node.

            // Sort the depth value from small to large.
            this.nodes.Sort((n1, n2) =>
            {
                if (n1.Depth == n2.Depth) // Larger child node first.
                    return n2.ChildrenCount - n1.ChildrenCount;

                return n1.Depth - n2.Depth;
            });

            // If there are more than one nodes for a given depth, the nodes are 
            // laid out one node per "row", starting from "row 0" which is on the 
            // top most. The "Node.DisplayRow" property carries this information.
            // 
            int displayRow = 0, currentDepth = -1;
            foreach (var node in this.nodes)
            {
                if (node.Depth != currentDepth)
                {
                    displayRow = 0;
                    currentDepth = node.Depth;
                }

                node.DisplayRow = displayRow++;
            }

            // Update the node positioning on canvas.
            this.nodes.ForEach(n => n.UpdateNodeLayout());
        }

        internal IEnumerable<string> NodesNotInGraph(SynthesizedGraph otherGraph)
        {
            var currentNodeIds = this.Nodes.Select(n => n.Identifier);
            var otherNodeIds = otherGraph.Nodes.Select(n => n.Identifier);
            return currentNodeIds.Except(otherNodeIds);
        }

        internal IEnumerable<KeyValuePair<string, int>> GetNodeDepths()
        {
            var depths = new Dictionary<string, int>();
            foreach (var node in this.nodes)
                depths.Add(node.Identifier, node.Depth);

            return depths;
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

        private void CalculateChildNode()
        {
            this.nodes.ForEach(n => n.ChildrenCount = -1);

            foreach (var node in this.nodes)
                GetChildNodeCount(node);
        }

        private int GetChildNodeCount(Node node)
        {
            // This node is already visited.
            if (node.ChildrenCount != -1)
                return node.ChildrenCount;

            // Mark this node as visited immediately.
            node.ChildrenCount = 0;

            int childNodeCount = 0;
            foreach (var childNode in GetChildNodes(node))
                childNodeCount += GetChildNodeCount(childNode) + 1;

            node.ChildrenCount = childNodeCount;
            return node.ChildrenCount;
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
