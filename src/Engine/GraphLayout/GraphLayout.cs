using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    public class Graph
    {
        public int MaxLayerHeight = 5;
        public double HorizontalNodeDistance = 100;
        public double VerticalNodeDistance = 30;

        public HashSet<Node> Nodes = new HashSet<Node>();
        public HashSet<Edge> Edges = new HashSet<Edge>();

        public List<List<Node>> Layers = new List<List<Node>>();

        #region Helper methods

        public void AddNode(Guid guid, double width, double height)
        {
            var node = new Node(guid, width, height, this);
            Nodes.Add(node);
        }

        public void AddEdge(Guid guid, Guid startId, Guid endId, double endY)
        {
            var edge = new Edge(guid, startId, endId, endY, this);
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

                        if (conn[xi, yi] + conn[yi, zi] + conn[xi, zi] == 3)
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
        /// This method implements Coffman-Graham layering algorithm.
        /// </summary>
        public void AssignLayers()
        {
            RemoveTransitiveEdges();

            // Label the nodes based on the number of incoming edges.
            List<Node> OrderedNodes = Nodes.OrderByDescending(
                x => x.LeftEdges.Count(e => e.Active)).ToList();
            HashSet<Node> LayeredNodes = new HashSet<Node>();

            Layers.Add(new List<Node>());
            int currentLayer = 0;
            int processed = 0;
            double layerWidth = 0;
            double previousLayerX = 0;

            while (processed < OrderedNodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.
                
                List<Node> selected = OrderedNodes.Where(x => x.Layer < 0 &&
                    x.RightEdges.Where(e => e.Active).All(e => e.EndNode.Layer >= 0)).ToList();

                Node n = selected.FirstOrDefault(x =>
                    x.RightEdges.All(e => e.EndNode.Layer < currentLayer) &&
                    x.LeftEdges.Count(e => e.Active) > 0);
                if (n == null) n = selected.First();

                // Add a new layer when needed
                if ((Layers[currentLayer].Count >= MaxLayerHeight) ||
                    !n.RightEdges.Where(e => e.Active).All(e => e.EndNode.Layer < currentLayer))
                {
                    // Horizontal node alignment for the previous layer
                    if (currentLayer > 0) previousLayerX = Layers[currentLayer - 1][0].X;
                    foreach (Node x in Layers[currentLayer])
                        x.X = previousLayerX - layerWidth - HorizontalNodeDistance;
                    
                    currentLayer++;
                    Layers.Add(new List<Node>());

                    layerWidth = 0;
                }

                n.Layer = currentLayer;
                Layers[currentLayer].Add(n);
                processed++;

                if (n.Width > layerWidth)
                    layerWidth = n.Width;
            }

            // Horizontal node alignment for the last (leftmost) layer
            if (currentLayer > 0) previousLayerX = Layers[currentLayer - 1][0].X;
            foreach (Node x in Layers[currentLayer])
                x.X = previousLayerX - layerWidth - HorizontalNodeDistance;

            Nodes = new HashSet<Node>(OrderedNodes);

            // Assign temporary vertical position for further processing
            foreach (List<Node> layer in Layers)
            {
                int y = 0;
                foreach (Node node in layer)
                {
                    node.Y = y;
                    y += 100;
                }
            }
        }

        /// <summary>
        /// Sugiyama step 3: Node Ordering
        /// This method uses Median heuristic to determine the vertical node
        /// order for each layer.
        /// </summary>
        public void OrderNodes()
        {
            List<Node> previous = null;
            foreach (List<Node> layer in Layers)
            {
                if (previous != null)
                {
                    foreach (Node n in layer)
                    {
                        // Get the temporary vertical coordinate of the median
                        // outgoing edge
                        List<Edge> neighborEdges = n.RightEdges.Where(e => e.Active)
                            .OrderBy(x => x.EndY).ToList();
                        if (neighborEdges.Count > 0)
                            n.Y = neighborEdges[neighborEdges.Count / 2].EndY;
                    }
                }

                // Sort the nodes on the layer by its temporary coordinates
                previous = layer.OrderBy(x => x.Y).ToList();
                Node top = null;
                foreach (Node n in previous)
                {
                    if (top == null)
                    {
                        top = n;
                        continue;
                    }

                    // Avoid vertical node overlapping
                    if (n.Y - top.Y < top.Height + VerticalNodeDistance)
                    {
                        n.Y = top.Y + top.Height + VerticalNodeDistance;
                    }

                    // Assign new coordinates to this node's incoming edges
                    int b = 1;
                    foreach (Edge e in n.LeftEdges)
                    {
                        e.EndY = n.Y + b;
                        b++;
                    }

                    top = n;
                }
            }
        }

        #endregion

        /// <summary>
        /// To align the top-left corner of the graph at coordinates (0,0).
        /// </summary>
        public void NormalizeGraphPosition()
        {
            double offset = Layers.Last().First().X;
            foreach (Node n in Nodes)
                n.X -= offset;
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

        public Node(Guid guid, double width, double height, Graph ownerGraph)
        {
            Id = guid;
            Width = width;
            Height = height;
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

        public bool Active = true;

        public Edge(Guid edgeId, Guid startId, Guid endId, double endY, Graph ownerGraph)
        {
            Id = edgeId;
            EndY = endY;
            OwnerGraph = ownerGraph;

            StartNode = OwnerGraph.FindNode(startId);
            if (StartNode != null)
                StartNode.RightEdges.Add(this);

            EndNode = OwnerGraph.FindNode(endId);
            if (EndNode != null)
                EndNode.LeftEdges.Add(this);
        }
    }
}
