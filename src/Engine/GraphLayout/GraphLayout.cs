using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    public class Graph
    {
        public const int MAX_LAYER_HEIGHT = 20;
        public const double HORIZONTAL_NODE_DISTANCE = 100;
        public const double VERTICAL_NODE_DISTANCE = 30;
        public const double INFINITE = 1000000;

        public HashSet<Node> Nodes = new HashSet<Node>();
        public HashSet<Edge> Edges = new HashSet<Edge>();

        public List<List<Node>> Layers = new List<List<Node>>();

        #region Helper methods

        public void AddNode(Guid guid, double width, double height, double y)
        {
            var node = new Node(guid, width, height, y, this);
            Nodes.Add(node);
        }

        public void AddEdge(Guid startId, Guid endId, double startY, double endY)
        {
            var edge = new Edge(startId, endId, startY, endY, this);
            Edges.Add(edge);
        }

        public Node FindNode(Guid guid)
        {
            foreach (Node node in Nodes)
                if (guid.Equals(node.Id))
                    return node;
            return null;
        }

        public Edge FindEdge(Node start, Node end)
        {
            foreach (Edge edge in Edges.Where(x => x.Active))
                if (start.Equals(edge.StartNode) && end.Equals(edge.EndNode))
                    return edge;
            return null;
        }

        public void AddToLayer(Node n, int currentLayer)
        {
            while (Layers.Count <= currentLayer)
                Layers.Add(new List<Node>());

            Layers[currentLayer].Add(n);
            n.Layer = currentLayer;
        }

        public void AddToLayer(List<Node> list, int currentLayer)
        {
            foreach (Node n in list)
                AddToLayer(n, currentLayer);
        }

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
                            conn[xi, yi] = FindEdge(x, y) != null ? 1 : -1;

                        if (conn[yi, zi] == 0)
                            conn[yi, zi] = FindEdge(y, z) != null ? 1 : -1;

                        Edge e = FindEdge(x, z);
                        if (conn[xi, zi] == 0)
                            conn[xi, zi] = e != null ? 1 : -1;

                        if (e != null && conn[xi, yi] + conn[yi, zi] + conn[xi, zi] == 3)
                            e.Active = false;

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

            // The rightmost layer is ordered based on the original vertical
            // position of the nodes.
            if (Edges.Count > 0)
                AddToLayer(Nodes.Where(x => x.RightEdges.Count == 0
                    && x.LeftEdges.Count > 0).OrderBy(x => x.Y).ToList(), 0);
            else
                AddToLayer(Nodes.OrderBy(x => x.Y).ToList(), 0);

            // Label the rest of the nodes based on the number of incoming edges.
            List<Node> OrderedNodes = Nodes.Where(x => x.LeftEdges.Count > 0)
                .OrderByDescending(x => x.LeftEdges.Count).ToList();

            bool isFinalLayer = false;
            int currentLayer = 0;
            int processed = Layers.Count > 0 ? Layers.First().Count : 0;

            double layerWidth = 0;

            while (processed < OrderedNodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.
                
                List<Node> selected = OrderedNodes.Where(x => x.Layer < 0 &&
                    x.RightEdges.All(e => e.EndNode.Layer >= 0)).ToList();

                Node n = null;

                if (selected.Count > 0)
                {
                    n = selected.FirstOrDefault(x =>
                        x.RightEdges.All(e => e.EndNode.Layer < currentLayer) &&
                        x.LeftEdges.Count(e => e.Active) > 0);

                    if (n == null)
                        n = selected.OrderByDescending(x => x.LeftEdges.Count).First();

                    if (n.LeftEdges.Count == 0)
                    {
                        Node temp = OrderedNodes.FirstOrDefault(x => x.Layer < 0 && x.LeftEdges.Count > 0);
                        if (temp != null)
                            n = temp;
                    }
                }
                else
                {
                    // For cyclic subgraphs
                    n = OrderedNodes.Where(x => x.Layer < 0).OrderByDescending(
                        x => x.RightEdges.Count(e => e.EndNode.Layer >= 0)).First();
                    currentLayer = 0;
                }

                // Add a new layer when needed
                if ((Layers.Count > 0 && Layers[currentLayer].Count >= MAX_LAYER_HEIGHT) ||
                    !n.RightEdges.All(e => e.EndNode.Layer < currentLayer) ||
                    (currentLayer > 0 && n.LeftEdges.Count == 0 && !isFinalLayer))
                {
                    currentLayer++;

                    layerWidth = 0;

                    if (n.LeftEdges.Count == 0)
                        isFinalLayer = true;
                }

                AddToLayer(n, currentLayer);
                processed++;

                if (n.Width > layerWidth)
                    layerWidth = n.Width;
            }

            // Put the input nodes on the leftmost layer
            AddToLayer(Nodes.Where(x => x.LeftEdges.Count == 0).ToList(), Layers.Count);
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
            foreach (List<Node> layer in Layers)
            {
                foreach (Node node in layer)
                    node.Y = INFINITE;
            }
            double y = 0;
            foreach (Node node in Layers.First())
            {
                node.Y = y;
                y += 80;
            }
            
            foreach (List<Node> layer in Layers)
            {
                double prevY = -10;

                foreach (Node n in layer)
                {
                    // Get the vertical coordinates from each node's median
                    // outgoing edge

                    if (layer.First().Layer > 0)
                    {
                        List<Edge> neighborEdges = n.RightEdges
                            .Where(x => x.EndNode.Y < INFINITE).OrderBy(x => x.EndY).ToList();

                        if (neighborEdges.Count > 1 && neighborEdges.Count % 2 == 0)
                        {
                            Edge median1 = neighborEdges[(neighborEdges.Count - 1) / 2];
                            Edge median2 = neighborEdges[(neighborEdges.Count) / 2];

                            n.Y = (median1.EndNode.Y + median1.EndOffsetY +
                                median2.EndNode.Y + median2.EndOffsetY -
                                median1.StartOffsetY - median2.StartOffsetY) / 2;
                        }
                        else if (neighborEdges.Count > 0)
                        {
                            Edge median = neighborEdges[(neighborEdges.Count - 1) / 2];

                            n.Y = median.EndNode.Y + median.EndOffsetY - median.StartOffsetY;
                        }
                        else if (n.LeftEdges.Count > 0)
                        {
                            n.Y = prevY + 10;
                            prevY = n.Y;
                        }
                    }
                }

                AssignCoordinates(layer);
            }
        }

        /// <summary>
        /// Sugiyama step 4: Assign Coordinates
        /// Vertical coordinates for the nodes in a layer is assigned right after the
        /// order of nodes in that particular layer is determined.
        /// </summary>
        public void AssignCoordinates(List<Node> layer)
        {
            // Assign vertical coordinates to the main nodes
            List<Node> nodes = layer.Where(x => x.Y < INFINITE).OrderBy(x => x.Y).ToList();

            double minDistance = INFINITE;
            int minNodeIndex = -1;
            for (int i = 1; i < nodes.Count; i++)
            {
                double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].Height;
                if (distance < VERTICAL_NODE_DISTANCE)
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

                minDistance = INFINITE;
                minNodeIndex = -1;
                for (int i = 1; i < nodes.Count; i++)
                {
                    double distance = nodes[i].Y - nodes[i - 1].Y - nodes[i - 1].Height;
                    if (distance < VERTICAL_NODE_DISTANCE)
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
                nodes.Last().Y + nodes.Last().Height + VERTICAL_NODE_DISTANCE;

            nodes = layer.Where(x => x.Y >= INFINITE).ToList();

            foreach (Node n in nodes)
            {
                n.Y = lastY;
                lastY += n.Height + VERTICAL_NODE_DISTANCE;
            }

            foreach (Node n in layer)
            {
                // Assign dummy coordinates to node incoming edges
                int b = 1;
                foreach (Edge e in n.LeftEdges.OrderBy(x => x.EndY))
                {
                    e.EndY = n.Y + b;
                    b++;
                }
            }
        }

        #endregion

        /// <summary>
        /// To align the top and left bound of the graph at x = 0 and y = 0.
        /// </summary>
        public void NormalizeGraphPosition()
        {
            List<List<Node>> ReversedLayers = Layers;
            ReversedLayers.Reverse();

            double previousLayerX = 0;
            double offsetY = -Nodes.OrderBy(x => x.Y).First().Y;

            foreach (List<Node> layer in Layers)
            {
                double layerWidth = layer.Max(x => x.Width);

                foreach (Node x in layer)
                    x.X = previousLayerX;

                previousLayerX = layer.First().X + layerWidth + HORIZONTAL_NODE_DISTANCE;

                double maxY = (layer.Min(x => x.Y) >= INFINITE) ?
                    0 : layer.Min(x => x.Y);

                foreach (Node n in layer.OrderBy(x => x.Y))
                {
                    n.Y += offsetY;

                    if (n.Y >= INFINITE + offsetY)
                        n.Y = maxY;
                    
                    if (n.Y + n.Height > maxY)
                        maxY = n.Y + n.Height + VERTICAL_NODE_DISTANCE;
                }
            }
        }

    }

    public class Node
    {
        private Graph OwnerGraph;

        public Guid Id;

        public double Width;
        public double Height;

        public double X;
        public double Y;

        public int Layer = -1;

        public HashSet<Edge> LeftEdges = new HashSet<Edge>();
        public HashSet<Edge> RightEdges = new HashSet<Edge>();

        public Node(Guid guid, double width, double height, double y, Graph ownerGraph)
        {
            Id = guid;
            Width = width;
            Height = height;
            Y = y;
            OwnerGraph = ownerGraph;
        }
    }

    public class Edge
    {
        private Graph OwnerGraph;

        public Guid Id;

        public Node StartNode;
        public Node EndNode;

        public double EndY;
        public double StartOffsetY;
        public double EndOffsetY;

        public bool Active = true;

        public Edge(Guid startId, Guid endId, double startY, double endY, Graph ownerGraph)
        {
            EndY = endY;
            OwnerGraph = ownerGraph;

            StartNode = OwnerGraph.FindNode(startId);
            if (StartNode != null)
                StartNode.RightEdges.Add(this);

            EndNode = OwnerGraph.FindNode(endId);
            if (EndNode != null)
                EndNode.LeftEdges.Add(this);

            StartOffsetY = startY - StartNode.Y;
            EndOffsetY = endY - EndNode.Y;
        }
    }
}
