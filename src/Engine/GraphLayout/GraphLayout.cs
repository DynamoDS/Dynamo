using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphLayout
{
    /// <summary>
    /// Represents the graph object (a set of nodes and edges) in the GraphLayout algorithm.
    /// </summary>
    public class Graph
    {

        #region Graph properties

        private const int MaxLayerHeight = 20;
        private const double Infinite = 1000000;
        
        public readonly static double VerticalNodeDistance = 30;
        public readonly static double HorizontalNodeDistance = 100;
        public readonly static double VerticalNoteDistance = 5;

        /// <summary>
        /// Set of nodes in this graph.
        /// </summary>
        public HashSet<Node> Nodes = new HashSet<Node>();

        /// <summary>
        /// Set of edges relevant to this graph.
        /// </summary>
        public HashSet<Edge> Edges = new HashSet<Edge>();

        /// <summary>
        /// Layers 1 and onwards list the nodes in this graph ordered by layer,
        /// with smaller numbers to the right part of the graph.
        /// Layer 0 refers to outside nodes connected to the right of the first layer.
        /// </summary>
        public List<List<Node>> Layers = new List<List<Node>>();

        /// <summary>
        /// Edges connected to outside nodes to the left of this graph.
        /// </summary>
        public HashSet<Edge> AnchorLeftEdges = new HashSet<Edge>();

        /// <summary>
        /// Edges connected to outside nodes to the right of this graph.
        /// </summary>
        public HashSet<Edge> AnchorRightEdges = new HashSet<Edge>();

        /// <summary>
        /// Stores the GraphCenterX value before layout algorithm.
        /// </summary>
        private double InitialGraphCenterX;

        /// <summary>
        /// Stores the GraphCenterY value before layout algorithm.
        /// </summary>
        private double InitialGraphCenterY;

        /// <summary>
        /// Stores the vertical offset value to avoid subgraph overlap
        /// after running the layout algorithm.
        /// </summary>
        public double OffsetY = 0;

        /// <summary>
        /// Returns the x coordinate of the graph's center point.
        /// </summary>
        public double GraphCenterX
        {
            get { return (Nodes.Min(n => n.X) + Nodes.Max(n => n.X + n.Width)) / 2; }
        }

        /// <summary>
        /// Returns the y coordinate of the graph's center point.
        /// </summary>
        public double GraphCenterY
        {
            get { return (Nodes.Min(n => n.Y) + Nodes.Max(n => n.Y + n.TotalHeight)) / 2; }
        }

        #endregion

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
        /// <param name="startX">The x coordinate of the connector's left end point.</param>
        /// <param name="startY">The y coordinate of the connector's left end point.</param>
        /// <param name="endX">The x coordinate of the connector's right end point.</param>
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

        /// <summary>
        /// To assign all nodes back to layer -1.
        /// </summary>
        public void ResetLayers()
        {
            foreach (var layer in Layers)
            {
                foreach (var node in layer)
                {
                    node.Layer = -1;
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
        /// This method implements Coffman-Graham layering algorithm.
        /// Unconnected output nodes will be put on the rightmost layer and
        /// they will be ordered based on their original vertical positions.
        /// </summary>
        public void AssignLayers()
        {
            RemoveTransitiveEdges();

            foreach (var n in Nodes)
                AddToLayer(n.RightEdges.Where(e => !e.EndNode.IsSelected).Select(e => e.EndNode).ToList(), 0);

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
                    currentLayer = 1;
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
        /// order for each layer.
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
                bool layerUpdated = false;

                foreach (Node n in layer)
                {
                    // Get the vertical coordinates of each node's right edge
                    // and get the median from these values

                    List<Edge> neighborEdges = n.RightEdges
                        .Where(x => x.EndNode.Y < Infinite).OrderBy(x => x.EndY).ToList();

                    if (neighborEdges.Count > 1 && neighborEdges.Count % 2 == 0)
                    {
                        Edge median1 = neighborEdges[(neighborEdges.Count - 1) / 2];
                        Edge median2 = neighborEdges[(neighborEdges.Count) / 2];

                        n.Y = (median1.EndY + median2.EndY -
                            median1.StartOffsetY - median2.StartOffsetY) / 2;
                        prevY = n.Y;
                        layerUpdated = true;
                    }
                    else if (neighborEdges.Count > 0)
                    {
                        Edge median = neighborEdges[(neighborEdges.Count - 1) / 2];

                        n.Y = median.EndY - median.StartOffsetY;
                        prevY = n.Y;
                        layerUpdated = true;
                    }
                    else if (n.LeftEdges.Count > 0 && AnchorRightEdges.Count == 0)
                    {
                        n.Y = prevY + 10;
                        prevY = n.Y;
                        layerUpdated = true;
                    }
                }

                if (layerUpdated)
                {
                    AssignCoordinates(layer);
                }
            }

            // Assign left-anchored nodes
            foreach (List<Node> layer in Layers.Skip(1).Reverse())
            {
                bool layerUpdated = false;
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
                            layerUpdated = true;
                        }
                        else if (neighborEdges.Count > 0)
                        {
                            Edge median = neighborEdges[(neighborEdges.Count - 1) / 2];

                            n.Y = median.StartNode.Y + median.StartOffsetY - median.EndOffsetY;
                            layerUpdated = true;
                        }
                    }
                }
                if (layerUpdated)
                {
                    AssignCoordinates(layer);
                }
            }
        }

        /// <summary>
        /// Sugiyama step 4: Assign Coordinates
        /// Vertical coordinates for the nodes in a layer is assigned right after the
        /// ordering of nodes in that particular layer is determined.
        /// </summary>
        /// <param name="layer">The nodes in a layer to be assigned their coordinates.</param>
        public void AssignCoordinates(List<Node> layer)
        {
            // Assign vertical coordinates to the main nodes
            // If two nodes have the same Y coordinate,
            // follow the original ordering before graph layout.
            List<Node> nodes = layer.Where(x => x.Y < Infinite)
                .OrderBy(x => x.Y).ThenBy(x => x.InitialY).ToList();

            double minDistance = Infinite;
            int minNodeIndex = -1;
            for (int i = 1; i < nodes.Count; i++)
            {
                double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].TotalHeight;
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
                    double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].TotalHeight;
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
                nodes.Last().Y + nodes.Last().TotalHeight + VerticalNodeDistance;

            nodes = layer.Where(x => x.Y >= Infinite).ToList();

            foreach (Node n in nodes)
            {
                n.Y = lastY;
                lastY += n.TotalHeight + VerticalNodeDistance;
            }
        }

        #endregion

        #region Graph positioning methods

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

                double layerWidth = layer.Max(x => Math.Max(x.Width, x.NotesWidth));

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

                    if (n.Y + n.TotalHeight > maxY)
                    {
                        maxY = n.Y + n.TotalHeight + VerticalNodeDistance;
                    }
                }
            }
        }

        /// <summary>
        /// To shift the whole graph back to its original position.
        /// </summary>
        public void SetGraphPosition(bool isGroupLayout)
        {
            double moveX = 0;
            double moveY = 0;

            // If this is a group or a separate subgraph then retain original position
            if (isGroupLayout || AnchorLeftEdges.Count + AnchorRightEdges.Count == 0)
            {
                moveX = InitialGraphCenterX - GraphCenterX;
                moveY = InitialGraphCenterY - GraphCenterY;
            }
            else
            {
                // If the subgraph is anchored to any side then adjust the Y position
                double outsideY = AnchorLeftEdges.Select(e => e.StartY)
                    .Union(AnchorRightEdges.Select(e => e.EndY)).Average();
                double insideY = AnchorLeftEdges.Select(e => e.EndNode.Y + e.EndOffsetY)
                    .Union(AnchorRightEdges.Select(e => e.StartNode.Y + e.StartOffsetY)).Average();
                moveY = outsideY - insideY;

                if (AnchorRightEdges.Count == 0)
                {
                    // Anchored to the left
                    moveX = Math.Max(HorizontalNodeDistance - AnchorLeftEdges.Min(e => e.EndX - e.StartX),
                        InitialGraphCenterX - GraphCenterX);
                }
                else if (AnchorLeftEdges.Count == 0)
                {
                    // Anchored to the right
                    moveX = Math.Min(AnchorRightEdges.Min(e => e.EndX - e.StartX) - HorizontalNodeDistance,
                        InitialGraphCenterX - GraphCenterX);
                }
                else
                {
                    // Anchored to both sides
                    moveX = (AnchorLeftEdges.Max(e => e.StartX) +
                        AnchorRightEdges.Min(e => e.EndNode.X)) / 2 - GraphCenterX;
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
        public Guid Id { get; private set; }

        /// <summary>
        /// The width of the node view.
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// The height of the node view and its linked notes.
        /// </summary>
        public double TotalHeight { get { return Height + NotesHeight; } }

        /// <summary>
        /// The height of the node view only.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// The x coordinate of the node view.
        /// </summary>
        public double X;

        /// <summary>
        /// The y coordinate of the node view or the topmost note view linked to this node.
        /// </summary>
        public double Y;

        /// <summary>
        /// The initial Y coordinate of the node view.
        /// </summary>
        public double InitialY { get; private set; }

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

        /// <summary>
        /// The maximum width of this node's linked notes.
        /// </summary>
        public double NotesWidth { get; private set; }

        /// <summary>
        /// The total height of all notes linked to this node,
        /// including the vertical spaces.
        /// </summary>
        public double NotesHeight { get; private set; }

        /// <summary>
        /// Marks a note as linked to this node.
        /// </summary>
        /// <param name="note">Note to be linked.</param>
        /// <param name="noteWidth">The width of the note model.</param>
        /// <param name="noteHeight">The height of the note model.</param>
        public void LinkNote(object note, double noteWidth, double noteHeight)
        {
            LinkedNotes.Add(note);
            NotesHeight += noteHeight + Graph.VerticalNoteDistance;
            NotesWidth = Math.Max(noteWidth, NotesWidth);

            // This Y refers to the topmost linked note's y coordinate if there is any
            Y -= noteHeight + Graph.VerticalNoteDistance;
        }

        /// <summary>
        /// True if the node is selected in the workspace.
        /// </summary>
        public bool IsSelected;

        public Node(Guid guid, double width, double height, double x, double y, bool isSelected, Graph ownerGraph)
        {
            Id = guid;
            Width = width;
            Height = height;
            X = x;
            Y = y;
            InitialY = y;
            IsSelected = isSelected;
            OwnerGraph = ownerGraph;
            NotesWidth = 0;
            NotesHeight = 0;
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
        public Node StartNode { get; private set; }

        /// <summary>
        /// The node connected to the edge's right end.
        /// </summary>
        public Node EndNode { get; private set; }

        /// <summary>
        /// Returns the x coordinate of the connector's start point.
        /// </summary>
        public double StartX
        {
            get { return StartNode.X + StartNode.Width; }
        }

        /// <summary>
        /// Returns the y coordinate of the connector's start point.
        /// </summary>
        public double StartY
        {
            get { return StartNode.Y + StartOffsetY; }
        }

        /// <summary>
        /// Returns the x coordinate of the connector's end point.
        /// </summary>
        public double EndX
        {
            get { return EndNode.X; }
        }

        /// <summary>
        /// Returns the y coordinate of the connector's end point.
        /// </summary>
        public double EndY
        {
            get { return EndNode.Y + EndOffsetY; }
        }

        /// <summary>
        /// The y distance between the edge's left end
        /// and the start node (including its linked notes)'s top.
        /// </summary>
        public double StartOffsetY { get { return NodeStartOffsetY + StartNode.NotesHeight; } }

        /// <summary>
        /// The y distance between the edge's left end and the start node's top-right corner.
        /// </summary>
        private double NodeStartOffsetY;

        /// <summary>
        /// The y distance between the edge's right end
        /// and the end node (including its linked notes)'s top.
        /// </summary>
        public double EndOffsetY { get { return NodeEndOffsetY + EndNode.NotesHeight; } }

        /// <summary>
        /// The y distance between the edge's right end and the end node's top-left corner.
        /// </summary>
        private double NodeEndOffsetY;

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

            NodeStartOffsetY = startY - StartNode.Y;
            NodeEndOffsetY = endY - EndNode.Y;
        }
    }
}
