using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    public class Graph
    {
        public int MaxWidth = 5;
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

        #endregion

        #region Sugiyama algorithm methods

        public void RemoveCycles()
        {
            // This method implements the Enhanced Greedy Cycle Removal heuristic.

            HashSet<Node> RemainingNodes = new HashSet<Node>(Nodes);
            HashSet<Edge> AcyclicEdges = new HashSet<Edge>();

            while (RemainingNodes.Count > 0)
            {
                // Remove all sink nodes
                HashSet<Node> selected = new HashSet<Node>(RemainingNodes.Where(n => n.RightEdges.Count(x => x.Active) == 0));
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
                selected = new HashSet<Node>(RemainingNodes.Where(n => n.LeftEdges.Count(x => x.Active) == 0));
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

                // Remove one node with the most outgoing edges
                if (RemainingNodes.Count > 0)
                {
                    int max = RemainingNodes.Max(x => x.RightEdges.Count(y => y.Active) - x.LeftEdges.Count(y => y.Active));
                    Node n = RemainingNodes.First(x => x.RightEdges.Count(y => y.Active) - x.LeftEdges.Count(y => y.Active) == max);

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

        public void RemoveTransitiveEdges()
        {
            // This method implements a simple transitive reduction using adjacency matrix.
            
            int N = Nodes.Count;
            int[,] conn = new int[N+1, N+1];

            int xi = 0, yi, zi;
            foreach (var x in Nodes)
            {
                xi++;
                yi = 0;
                foreach (var y in Nodes)
                {
                    yi++;
                    zi = 0;
                    if (x == y) continue;
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
                        {
                            e.Active = false;
                        }
                    }
                }
            }
        }

        public void AssignLayers()
        {
            // This method implements Coffman-Graham layering algorithm.
            
            List<Node> OrderedNodes = Nodes.OrderByDescending(x => x.LeftEdges.Count(e => e.Active)).ToList();
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

                Node n = selected.FirstOrDefault(x => x.RightEdges.All(e => e.EndNode.Layer < currentLayer) && x.LeftEdges.Count(e => e.Active) > 0);
                if (n == null) n = selected.First();

                // Add new layer when needed
                if ((Layers[currentLayer].Count >= MaxWidth) || !n.RightEdges.Where(e => e.Active).All(e => e.EndNode.Layer < currentLayer))
                {
                    // Horizontal node alignment for the last layer
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

            // Horizontal node alignment for the last layer
            if (currentLayer > 0) previousLayerX = Layers[currentLayer - 1][0].X;
            foreach (Node x in Layers[currentLayer])
                x.X = previousLayerX - layerWidth - HorizontalNodeDistance;

            Nodes = new HashSet<Node>(OrderedNodes);

            // Temporary vertical position for further processing
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

        public void OrderNodes()
        {
            // This method uses Median heuristic to determine the vertical node order on each layer.

            List<Node> previous = null;
            foreach (List<Node> layer in Layers)
            {
                if (previous != null)
                {
                    foreach (Node n in layer)
                    {
                        List<Edge> neighborEdges = n.RightEdges.Where(e => e.Active)
                            .OrderBy(x => x.EndY).ToList();
                        if (neighborEdges.Count > 0)
                            n.Y = neighborEdges[neighborEdges.Count / 2].EndY;
                    }
                }

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

                    int b = 2;
                    foreach (Edge e in n.LeftEdges)
                    {
                        e.EndY = n.Y + b;
                        b += 2;
                    }

                    top = n;
                }
            }
        }

        #endregion

        /// <summary>
        /// To align the top-left corner of the graph at (x=0, y=0).
        /// </summary>
        public void NormalizeGraphPosition()
        {
            double offsetX = -Layers.Last().First().X;
            foreach (Node n in Nodes)
                n.X += offsetX;
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
