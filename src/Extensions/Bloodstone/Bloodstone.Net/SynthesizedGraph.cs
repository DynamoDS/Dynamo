using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Bloodstone
{
    struct Config
    {
        internal static readonly double HorzGap = 32.0;
        internal static readonly double VertGap = 16.0;
        internal static readonly double NodeWidth = 128.0;
        internal static readonly double NodeHeight = 12.0;
        internal static readonly double HorzSpace = ((2 * HorzGap) + NodeWidth);
    }

    public interface ISynthesizedGraph
    {
        void AddNode(string identifier, string name);
        void AddEdge(string startNodeId, string endNodeId);
    }

    internal class Node
    {
        private Rect rect = new Rect();

        internal Node(string identifier, string name)
        {
            this.Identifier = identifier;
            this.Name = name;
            this.UpstreamNodeCount = -1;
            this.InputCount = this.OutputCount = 0;
        }

        internal double UpdateNodeLayout(double nodeTopCoord)
        {
            // Pick the biggest between input and output count. And if that is 
            // zero, then we will default to 1 so that the node will not be tiny.
            var portCount = Math.Max(1, Math.Max(this.InputCount, this.OutputCount));
            var height = ((portCount + 1) * Config.NodeHeight);

            var left = (Config.HorzGap + (Config.HorzSpace * Depth));
            this.rect = new Rect(left, nodeTopCoord, Config.NodeWidth, height);
            return height;
        }

        internal void UpdateNodeIndex(int index)
        {
            this.NodeIndex = index;
        }

        internal Point GetInputPoint(int index)
        {
            var offset = ((index + 1) * Config.NodeHeight);
            return new Point(rect.Left, rect.Top + offset);
        }

        internal Point GetOutputPoint(int index)
        {
            var offset = ((index + 1) * Config.NodeHeight);
            return new Point(rect.Right, rect.Top + offset);
        }

        #region Public Class Properties

        internal int NodeIndex { get; private set; }
        internal int Depth { get; set; }
        internal int DisplayRow { get; set; }
        internal int UpstreamNodeCount { get; set; }
        internal int InputCount { get; set; }
        internal int OutputCount { get; set; }
        internal string Identifier { get; private set; }
        internal string Name { get; private set; }
        internal Rect Rect { get { return this.rect; } }

        #endregion
    }

    internal class Edge
    {
        internal Edge(string startNodeId, string endNodeId)
        {
            this.StartNodeId = startNodeId;
            this.EndNodeId = endNodeId;
        }

        internal int StartIndex { get; set; }
        internal int EndIndex { get; set; }
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
            LabelGraphNodes();    // Label the entire graph with depth info.
            CalculateUpstreamNode(); // Compute number of upstreams for each node.
            AssignEdgeIndices();  // Assign an index for each edge based on position.

            // Sort the depth value from small to large.
            this.nodes.Sort((n1, n2) =>
            {
                if (n1.Depth == n2.Depth) // Larger parent node first.
                    return n2.UpstreamNodeCount - n1.UpstreamNodeCount;

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
            int depth = -1, index = 0;
            double nodeTopCoord = Config.VertGap;
            foreach (var node in this.nodes)
            {
                if (depth != node.Depth)
                {
                    depth = node.Depth;
                    nodeTopCoord = Config.VertGap;
                }

                node.UpdateNodeIndex(index++);
                double height = node.UpdateNodeLayout(nodeTopCoord);
                nodeTopCoord = nodeTopCoord + height + Config.VertGap;
            }
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

        internal IEnumerable<KeyValuePair<string, Color>> GetNodeColors()
        {
            var depths = new Dictionary<string, Color>();
            foreach (var node in this.nodes)
            {
                var brush = GraphResources.NodeColor(node.NodeIndex);
                depths.Add(node.Identifier, brush.Color);
            }

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
            // Reset the depth value of each node to maximum value.
            this.nodes.ForEach(n => n.Depth = int.MaxValue);

            // Get a list of all leaf nodes.
            var leafNodes = nodes.Where(n =>
            {
                // A leaf node is a node which doesn't have output edge.
                return !(edges.Where(e => e.StartNodeId == n.Identifier).Any());
            });

            foreach (var leafNode in leafNodes)
                LabelGraphNode(leafNode, 0);

            // Reset all node depth values so that 
            // they start from zero and move upwards.
            int minDepth = nodes.Min(n => n.Depth);
            nodes.ForEach(n => n.Depth = n.Depth - minDepth);
        }

        private void LabelGraphNode(Node node, int depth)
        {
            // The node is already marked with a smaller depth value, which 
            // means this node was visited before this call with much longer
            // chain of descendant list. No point going further to label them.
            // 
            if (node.Depth < depth)
                return;

            node.Depth = depth;
            var upstreamdNodes = GetUpstreamNodes(node);
            foreach (var upstreamNode in upstreamdNodes)
                LabelGraphNode(upstreamNode, depth - 1);
        }

        private void CalculateUpstreamNode()
        {
            this.nodes.ForEach(n => n.UpstreamNodeCount = -1);

            foreach (var node in this.nodes)
                GetUpstreamNodeCount(node);
        }

        private int GetUpstreamNodeCount(Node node)
        {
            // This node is already visited.
            if (node.UpstreamNodeCount != -1)
                return node.UpstreamNodeCount;

            // Mark this node as visited immediately.
            node.UpstreamNodeCount = 0;

            foreach (var upstreamNode in GetUpstreamNodes(node))
                node.UpstreamNodeCount += GetUpstreamNodeCount(upstreamNode) + 1;

            return node.UpstreamNodeCount;
        }

        private IEnumerable<Node> GetUpstreamNodes(Node node)
        {
            var startNodeQuery = this.edges.Where((e) =>
            {
                // Get edges connecting to this "node".
                return e.EndNodeId == node.Identifier;

            }).Select(e => e.StartNodeId);

            return this.nodes.Where((n) =>
            {
                // A node will be considered a upstream node 
                // if it is at the start of connecting edges.
                return startNodeQuery.Contains(n.Identifier);
            });
        }

        private void AssignEdgeIndices()
        {
            var nodeInputs = new Dictionary<string, int>();
            var nodeOutputs = new Dictionary<string, int>();
            this.edges.ForEach(e => e.StartIndex = e.EndIndex = -1);

            foreach (var edge in this.edges)
            {
                var startNodeId = edge.StartNodeId;
                var endNodeId = edge.EndNodeId;

                edge.StartIndex = 0;
                if (nodeOutputs.Keys.Contains(startNodeId) != false)
                    edge.StartIndex = nodeOutputs[startNodeId] + 1;

                edge.EndIndex = 0;
                if (nodeInputs.Keys.Contains(endNodeId) != false)
                    edge.EndIndex = nodeInputs[endNodeId] + 1;

                nodeOutputs[startNodeId] = edge.StartIndex;
                nodeInputs[endNodeId] = edge.EndIndex;
            }

            this.nodes.ForEach((n) =>
            {
                n.InputCount = n.OutputCount = 0;

                int dummyInputCount, dummyOutputCount;
                if (nodeInputs.TryGetValue(n.Identifier, out dummyInputCount))
                    n.InputCount = dummyInputCount + 1;

                if (nodeOutputs.TryGetValue(n.Identifier, out dummyOutputCount))
                    n.OutputCount = dummyOutputCount + 1;
            });
        }

        #endregion
    }
}
