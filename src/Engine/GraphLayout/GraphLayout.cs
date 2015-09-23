using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    /// <summary>
    /// Represents the graph object (a set of nodes and edges) in the GraphLayout algorithm.
    /// </summary>
    public class Graph
    {
        private const int MaxLayerHeight = 20;
        private const double HorizontalNodeDistance = 100;
        private const double VerticalNodeDistance = 30;
        private const double Infinite = 1000000;

        public HashSet<Node> Nodes = new HashSet<Node>();
        public HashSet<Edge> Edges = new HashSet<Edge>();

        public Graph ParentGraph;

        public HashSet<Edge> AnchorLeftEdges = new HashSet<Edge>();
        public HashSet<Edge> AnchorRightEdges = new HashSet<Edge>();

        public List<List<Node>> Layers = new List<List<Node>>();

        #region Helper methods

        /// <summary>
        /// Adds a new node to the graph object.
        /// </summary>
        /// <param name="guid">The guid as a unique identifier of the node.</param>
        /// <param name="width">The width of the node view.</param>
        /// <param name="height">The height of the node view.</param>
        /// <param name="x">The x coordinate of the node view.</param>
        /// <param name="y">The y coordinate of the node view.</param>
        /// <param name="isSelected">True if the node is selected in the workspace.</param>
        public void AddNode(Guid guid, double width, double height, double x, double y, bool isSelected)
        {
            var node = new Node(guid, width, height, x, y, isSelected, this);
            Nodes.Add(node);
        }

        /// <summary>
        /// Adds a new edge to the graph object.
        /// </summary>
        /// <param name="startId">The guid of the starting node.</param>
        /// <param name="endId">The guid of the ending node.</param>
        /// <param name="startY">The y coordinate of the connector's left end point.</param>
        /// <param name="endY">The y coordinate of the connector's right end point.</param>
        public void AddEdge(Guid startId, Guid endId, double startX, double startY, double endX, double endY)
        {
            var edge = new Edge(startId, endId, startX, startY, endX, endY, this);
            Edges.Add(edge);
        }

        /// <summary>
        /// Finds a node from its unique guid.
        /// </summary>
        /// <param name="guid">The node's guid.</param>
        /// <returns>The node object.</returns>
        public Node FindNode(Guid guid)
        {
            foreach (Node node in Nodes)
            {
                if (guid.Equals(node.Id))
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds an active edge between two nodes.
        /// </summary>
        /// <param name="start">Start node.</param>
        /// <param name="end">End node.</param>
        /// <returns>The edge object.</returns>
        public Edge FindEdge(Node start, Node end)
        {
            foreach (Edge edge in Edges)
            {
                if (edge.Active && start.Equals(edge.StartNode) && end.Equals(edge.EndNode))
                {
                    return edge;
                }
            }
            return null;
        }

        /// <summary>
        /// Assigns a node into a vertical layer in the graph.
        /// </summary>
        /// <param name="n">The node.</param>
        /// <param name="currentLayer">The number of the layer, starting from 0 for the rightmost layer.</param>
        public void AddToLayer(Node n, int currentLayer)
        {
            while (Layers.Count <= currentLayer)
                Layers.Add(new List<Node>());

            Layers[currentLayer].Add(n);
            n.Layer = currentLayer;
        }

        /// <summary>
        /// Assigns a list of nodes into a vertical layer in the graph.
        /// </summary>
        /// <param name="list">The list of nodes.</param>
        /// <param name="currentLayer">The number of the layer, starting from 0 for the rightmost layer.</param>
        public void AddToLayer(List<Node> list, int currentLayer)
        {
            foreach (Node n in list)
                AddToLayer(n, currentLayer);
        }

        /// <summary>
        /// Removes any transitive edges in the graph.
        /// </summary>
        public void RemoveTransitiveEdges()
        {
            // Check for transitive edges using an adjacency matrix.
            // For the matrix cells, 1 implies that there exists a path between
            // nodes A and B, or -1 otherwise.

            int[,] conn = new int[Nodes.Count + 1, Nodes.Count + 1];
            int xi, yi, zi;

            xi = 0;
            foreach (var x in Nodes)
            {
                xi++;

                yi = 0;
                foreach (var y in Nodes)
                {
                    yi++;
                    if (x == y) continue;

                    zi = 0;
                    foreach (var z in Nodes)
                    {
                        zi++;
                        if (x == z) continue;
                        if (y == z) continue;

                        if (conn[xi, yi] == 0)
                        {
                            conn[xi, yi] = FindEdge(x, y) != null ? 1 : -1;
                        }

                        if (conn[yi, zi] == 0)
                        {
                            conn[yi, zi] = FindEdge(y, z) != null ? 1 : -1;
                        }

                        Edge e = FindEdge(x, z);
                        if (conn[xi, zi] == 0)
                        {
                            conn[xi, zi] = e != null ? 1 : -1;
                        }

                        if (e != null && conn[xi, yi] + conn[yi, zi] + conn[xi, zi] == 3)
                        {
                            e.Active = false;
                        }

                    }
                }
            }
        }

        #endregion

        #region Sugiyama algorithm methods

        /// <summary>
        /// Sugiyama step 1: Cycle Removal
        /// This method implements an enhanced Greedy Cycle Removal heuristic
        /// proposed by Eades et al, 1993.
        /// http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.47.7745
        /// </summary>
        public void RemoveCycles()
        {
            HashSet<Node> RemainingNodes = new HashSet<Node>(Nodes);
            HashSet<Edge> AcyclicEdges = new HashSet<Edge>();

            while (RemainingNodes.Count > 0)
            {
                // Remove all sink nodes
                HashSet<Node> selected = new HashSet<Node>(RemainingNodes.Where(
                    n => n.RightEdges.Count(x => x.Active) == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.LeftEdges.Where(x => x.Active))
                    {
                        AcyclicEdges.Add(e);
                        e.Active = false;
                    }
                }

                // Remove all isolated nodes
                RemainingNodes.ExceptWith(selected);

                // Remove all source nodes
                selected = new HashSet<Node>(RemainingNodes.Where(
                    n => n.LeftEdges.Count(x => x.Active) == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.RightEdges.Where(x => x.Active))
                    {
                        AcyclicEdges.Add(e);
                        e.Active = false;
                    }
                }

                // Remove all isolated nodes
                RemainingNodes.ExceptWith(selected);

                // Remove one node with the largest number of outgoing edges
                if (RemainingNodes.Count > 0)
                {
                    int max = RemainingNodes.Max(x => x.RightEdges.Count(y => y.Active)
                        - x.LeftEdges.Count(y => y.Active));
                    Node n = RemainingNodes.First(x => x.RightEdges.Count(y => y.Active)
                        - x.LeftEdges.Count(y => y.Active) == max);

                    AcyclicEdges.UnionWith(n.RightEdges.Where(x => x.Active));

                    foreach (Edge e in n.RightEdges)
                        e.Active = false;

                    foreach (Edge e in n.LeftEdges)
                        e.Active = false;

                    RemainingNodes.Remove(n);
                }
            }

            Edges = AcyclicEdges;

            foreach (Edge e in Edges)
                e.Active = true;
        }
        
        /// <summary>
        /// Sugiyama step 2: Layering
        /// This method implements Coffman-Graham layering algorithm.  All
        /// output nodes will be put on the rightmost layer and they will be
        /// ordered based on their original vertical positions.  All input
        /// nodes will be put on the leftmost layer, and all isolated nodes
        /// will be put below the input nodes.  This includes an implementation
        /// to avoid horizontal node overlapping.
        /// </summary>
        public void AssignLayers()
        {
            RemoveTransitiveEdges();

            foreach (var n in Nodes)
            {
                AddToLayer(n.RightEdges.Where(e => !e.EndNode.IsSelected).Select(e => e.EndNode).ToList(), 0);
            }

            int currentLayer = 1;
            int processed = 0;

            while (processed < Nodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.

                List<Node> selected = Nodes.Where(x => x.Layer < 0 &&
                    x.RightEdges.All(e => e.EndNode.Layer >= 0)).OrderBy(x => x.Y).ToList();

                Node n = null;

                if (selected.Count > 0)
                {
                    n = selected.FirstOrDefault(
                        x => x.RightEdges.All(e => e.EndNode.Layer < currentLayer))
                        ?? selected.OrderByDescending(x => x.LeftEdges.Count).First();
                }
                else
                {
                    // For cyclic subgraphs
                    n = Nodes.Where(x => x.Layer < 0).OrderByDescending(
                        x => x.RightEdges.Count(e => e.EndNode.Layer >= 0)).First();
                    currentLayer = 0;
                }

                // Add a new layer when needed
                if ((Layers.Count > 1 && Layers[currentLayer].Count >= MaxLayerHeight) ||
                    !n.RightEdges.All(e => e.EndNode.Layer < currentLayer))
                {
                    currentLayer++;
                }

                AddToLayer(n, currentLayer);
                processed++;
            }
        }

        /// <summary>
        /// Sugiyama step 3: Node Ordering
        /// This method uses Median heuristic to determine the vertical node
        /// order for each layer.  This includes an implementation to avoid
        /// vertical node overlapping.
        /// </summary>
        public void OrderNodes()
        {
            // Assign temporary vertical indices for further processing
            foreach (List<Node> layer in Layers.Skip(1))
            {
                foreach (Node node in layer)
                    node.Y = Infinite;
            }
            
            foreach (List<Node> layer in Layers.Skip(1))
            {
                double prevY = -10;
                bool changed = false;

                foreach (Node n in layer)
                {
                    // Get the vertical coordinates from each node's median
                    // outgoing edge

                    List<Edge> neighborEdges = n.RightEdges
                        .Where(x => x.EndNode.Y < Infinite).OrderBy(x => x.EndY).ToList();

                    if (neighborEdges.Count > 1 && neighborEdges.Count % 2 == 0)
                    {
                        Edge median1 = neighborEdges[(neighborEdges.Count - 1) / 2];
                        Edge median2 = neighborEdges[(neighborEdges.Count) / 2];

                        n.Y = (median1.EndNode.Y + median1.EndOffsetY +
                            median2.EndNode.Y + median2.EndOffsetY -
                            median1.StartOffsetY - median2.StartOffsetY) / 2;
                        prevY = n.Y;
                        changed = true;
                    }
                    else if (neighborEdges.Count > 0)
                    {
                        Edge median = neighborEdges[(neighborEdges.Count - 1) / 2];

                        n.Y = median.EndNode.Y + median.EndOffsetY - median.StartOffsetY;
                        prevY = n.Y;
                        changed = true;
                    }
                    else if (n.LeftEdges.Count > 0 && AnchorRightEdges.Count == 0)
                    {
                        n.Y = prevY + 10;
                        prevY = n.Y;
                        changed = true;
                    }
                }

                if (changed)
                    AssignCoordinates(layer);
            }

            // Assign left-anchored nodes
            foreach (List<Node> layer in Layers.Skip(1))
            {
                bool changed = false;
                foreach (Node n in layer)
                {
                    if (n.Y >= Infinite)
                    {
                        List<Edge> neighborEdges = n.LeftEdges
                            .Where(x => x.StartNode.Y < Infinite).OrderBy(x => x.EndY).ToList();

                        if (neighborEdges.Count > 1 && neighborEdges.Count % 2 == 0)
                        {
                            Edge median1 = neighborEdges[(neighborEdges.Count - 1) / 2];
                            Edge median2 = neighborEdges[(neighborEdges.Count) / 2];

                            n.Y = (median1.StartNode.Y + median1.StartOffsetY +
                                median2.StartNode.Y + median2.StartOffsetY -
                                median1.EndOffsetY - median2.EndOffsetY) / 2;
                        }
                        else if (neighborEdges.Count > 0)
                        {
                            Edge median = neighborEdges[(neighborEdges.Count - 1) / 2];

                            n.Y = median.StartNode.Y + median.StartOffsetY - median.EndOffsetY;
                        }
                        changed = true;
                    }
                }
                if (changed)
                    AssignCoordinates(layer);
            }
        }

        /// <summary>
        /// Sugiyama step 4: Assign Coordinates
        /// Vertical coordinates for the nodes in a layer is assigned right after the
        /// order of nodes in that particular layer is determined.
        /// </summary>
        /// <param name="layer">The nodes in a layer to be assigned their coordinates.</param>
        public void AssignCoordinates(List<Node> layer)
        {
            // Assign vertical coordinates to the main nodes
            List<Node> nodes = layer.Where(x => x.Y < Infinite).OrderBy(x => x.Y).ToList();

            double minDistance = Infinite;
            int minNodeIndex = -1;
            for (int i = 1; i < nodes.Count; i++)
            {
                double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].Height;
                if (distance < VerticalNodeDistance)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minNodeIndex = i;
                    }
                }
            }

            while (minNodeIndex > -1)
            {
                nodes[minNodeIndex].Y += 1;
                nodes[minNodeIndex - 1].Y -= 1;

                minDistance = Infinite;
                minNodeIndex = -1;
                for (int i = 1; i < nodes.Count; i++)
                {
                    double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].Height;
                    if (distance < VerticalNodeDistance)
                    {
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minNodeIndex = i;
                        }
                    }
                }
            }

            // Assign vertical coordinates to the rest of the nodes
            double lastY = (nodes.Count == 0) ? 0 :
                nodes.Last().Y + nodes.Last().Height + VerticalNodeDistance;

            nodes = layer.Where(x => x.Y >= Infinite).ToList();

            foreach (Node n in nodes)
            {
                n.Y = lastY;
                lastY += n.Height + VerticalNodeDistance;
            }
        }

        #endregion

        #region Graph positioning methods

        private double InitialGraphCenterX;
        private double InitialGraphCenterY;
        public double OffsetY = 0;

        public double GraphCenterX
        {
            get { return (Nodes.Min(n => n.X) + Nodes.Max(n => n.X + n.Width)) / 2; }
        }

        public double GraphCenterY
        {
            get { return (Nodes.Min(n => n.Y) + Nodes.Max(n => n.Y + n.Height)) / 2; }
        }

        /// <summary>
        /// To save the initial center position of the graph.
        /// </summary>
        public void RecordInitialPosition()
        {
            InitialGraphCenterX = GraphCenterX;
            InitialGraphCenterY = GraphCenterY;
        }

        /// <summary>
        /// To set spaces between the nodes based on the default node distance.
        /// </summary>
        public void DistributeNodePosition()
        {
            double previousLayerX = 0;
            double offsetY = -Nodes.OrderBy(x => x.Y).First().Y;

            foreach (List<Node> layer in Layers.Skip(1).AsEnumerable().Reverse())
            {
                if (layer.Count == 0) continue;

                double layerWidth = layer.Max(x => x.Width);

                foreach (Node x in layer)
                    x.X = previousLayerX;

                previousLayerX = layer.First().X + layerWidth + HorizontalNodeDistance;

                double maxY = (layer.Min(x => x.Y) >= Infinite) ?
                    0 : layer.Min(x => x.Y);

                foreach (Node n in layer.OrderBy(x => x.Y))
                {
                    n.Y += offsetY;

                    if (n.Y >= Infinite + offsetY)
                    {
                        n.Y = maxY;
                    }

                    if (n.Y + n.Height > maxY)
                    {
                        maxY = n.Y + n.Height + VerticalNodeDistance;
                    }
                }
            }
        }

        /// <summary>
        /// To shift the whole graph back to its original position.
        /// </summary>
        public void SetGraphPosition()
        {
            double moveX = 0;
            double moveY = 0;

            // If this is a separate subgraph then retain original position
            if (AnchorLeftEdges.Count + AnchorRightEdges.Count == 0)
            {
                moveX = InitialGraphCenterX - GraphCenterX;
                moveY = InitialGraphCenterY - GraphCenterY;
            }
            else
            {
                double outsideY = AnchorLeftEdges.Select(e => e.StartY)
                    .Union(AnchorRightEdges.Select(e => e.EndY)).Average();
                double insideY = AnchorLeftEdges.Select(e => e.EndNode.Y + e.EndOffsetY)
                    .Union(AnchorRightEdges.Select(e => e.StartNode.Y + e.StartOffsetY)).Average();
                moveY = outsideY - insideY;
                //moveY = InitialGraphCenterY - GraphCenterY;

                if (AnchorRightEdges.Count == 0)
                {
                    // Anchored to the left
                    double outsideX = AnchorLeftEdges.Max(e => e.StartX);
                    double insideX = Nodes.Min(n => n.X);
                    moveX = outsideX - insideX + HorizontalNodeDistance;

                    moveX = Math.Max(HorizontalNodeDistance - AnchorLeftEdges.Min(e => e.EndX - e.StartX),
                        InitialGraphCenterX - GraphCenterX);
                }
                else if (AnchorLeftEdges.Count == 0)
                {
                    // Anchored to the right
                    double outsideX = AnchorRightEdges.Min(e => e.EndNode.X);
                    double insideX = Nodes.Max(n => n.X + n.Width);
                    moveX = outsideX - insideX - HorizontalNodeDistance;

                    moveX = Math.Min(AnchorRightEdges.Min(e => e.EndX - e.StartX) - HorizontalNodeDistance,
                        InitialGraphCenterX - GraphCenterX);
                }
                else
                {
                    // Anchored to both sides
                    double outsideX = (AnchorLeftEdges.Max(e => e.StartX) +
                        AnchorRightEdges.Min(e => e.EndNode.X)) / 2;
                    moveX = outsideX - GraphCenterX;
                }
            }

            foreach (Node n in Nodes)
            {
                n.X += moveX;
                n.Y += moveY;
            }
        }

        #endregion

    }

    /// <summary>
    /// Represents a node/vertex object in the GraphLayout algorithm.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The graph object which owns the node.
        /// </summary>
        private Graph OwnerGraph;

        /// <summary>
        /// The unique identifier of the node.
        /// </summary>
        public Guid Id;

        /// <summary>
        /// The width of the node view.
        /// </summary>
        public double Width;

        /// <summary>
        /// The height of the node view.
        /// </summary>
        public double Height;

        /// <summary>
        /// The x coordinate of the node view.
        /// </summary>
        public double X;

        /// <summary>
        /// The y coordinate of the node view.
        /// </summary>
        public double Y;

        /// <summary>
        /// The layer of the node within the graph, starting from layer 0 for the rightmost layer.
        /// </summary>
        public int Layer = -1;

        /// <summary>
        /// The set of edges connected to the input ports the node.
        /// </summary>
        public HashSet<Edge> LeftEdges = new HashSet<Edge>();

        /// <summary>
        /// The set of edges connected to the output ports of the node.
        /// </summary>
        public HashSet<Edge> RightEdges = new HashSet<Edge>();

        /// <summary>
        /// A list of note models which has this node as the closest node.
        /// </summary>
        public List<Object> LinkedNotes = new List<Object>();

        public bool IsSelected;

        public Node(Guid guid, double width, double height, double x, double y, bool isSelected, Graph ownerGraph)
        {
            Id = guid;
            Width = width;
            Height = height;
            X = x;
            Y = y;
            IsSelected = isSelected;
            OwnerGraph = ownerGraph;
        }
    }

    /// <summary>
    /// Represents an edge/link object in the GraphLayout algorithm.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// The graph object which owns the edge.
        /// </summary>
        private Graph OwnerGraph;

        /// <summary>
        /// The node connected to the edge's left end.
        /// </summary>
        public Node StartNode;

        /// <summary>
        /// The node connected to the edge's right end.
        /// </summary>
        public Node EndNode;

        public double StartX
        {
            get { return StartNode.X + StartNode.Width;  }
        }
        public double StartY
        {
            get { return StartNode.Y + StartOffsetY; }
        }
        public double EndX
        {
            get { return EndNode.X; }
        }
        public double EndY
        {
            get { return EndNode.Y + EndOffsetY; }
        }

        /// <summary>
        /// The y distance between the edge's left end and the start node's top-right corner.
        /// </summary>
        public double StartOffsetY;

        /// <summary>
        /// The y distance between the edge's right end and the end node's top-left corner.
        /// </summary>
        public double EndOffsetY;

        /// <summary>
        /// A flag for the GraphLayout algorithm.
        /// </summary>
        public bool Active = true;

        public Edge(Guid startId, Guid endId, double startX, double startY, double endX, double endY, Graph ownerGraph)
        {
            OwnerGraph = ownerGraph;

            StartNode = OwnerGraph.FindNode(startId);
            if (StartNode != null)
            {
                StartNode.RightEdges.Add(this);
            }

            EndNode = OwnerGraph.FindNode(endId);
            if (EndNode != null)
            {
                EndNode.LeftEdges.Add(this);
            }

            StartOffsetY = startY - StartNode.Y;
            EndOffsetY = endY - EndNode.Y;
        }
    }
}
