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

        public HashSet<Node> Nodes = new HashSet<Node>();
        public HashSet<Edge> Edges = new HashSet<Edge>();

        public List<List<Node>> Layers = new List<List<Node>>();

        public void AddNode(Guid guid, double width, double height)
        {
            var node = new Node(guid, width, height, this);
            Nodes.Add(node);
        }

        public void AddEdge(Guid guid, Guid start, Guid end, double endX, double endY)
        {
            var edge = new Edge(guid, start, end, endX, endY, this);
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

        private int GetLayerWidth(int layer)
        {
            return Nodes.Count(x => x.Layer == layer);
        }

        public bool RemoveCycles()
        {
            // This method implements Greedy Cycle Removal heuristic.

            HashSet<Node> BackupNodes = new HashSet<Node>(Nodes);
            HashSet<Edge> AcyclicEdges = new HashSet<Edge>();

            while (Nodes.Count > 0)
            {
                // Remove all sink nodes
                HashSet<Node> selected = new HashSet<Node>(Nodes.Where(n => n.RightEdges.Count(x => x.Active) == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.LeftEdges.Where(x => x.Active))
                    {
                        AcyclicEdges.Add(e);
                        e.Active = false;
                    }
                }

                // Remove all isolated nodes
                Nodes.ExceptWith(selected);

                // Remove all source nodes
                selected = new HashSet<Node>(Nodes.Where(n => n.LeftEdges.Count(x => x.Active) == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.RightEdges.Where(x => x.Active))
                    {
                        AcyclicEdges.Add(e);
                        e.Active = false;
                    }
                }

                // Remove all isolated nodes
                Nodes.ExceptWith(selected);

                // if G is not empty then
                if (Nodes.Count > 0)
                {
                    int max = Nodes.Max(x => x.RightEdges.Count(y => y.Active) - x.LeftEdges.Count(y => y.Active));
                    Node n = Nodes.First(x => x.RightEdges.Count(y => y.Active) - x.LeftEdges.Count(y => y.Active) == max);

                    AcyclicEdges.UnionWith(n.RightEdges.Where(x => x.Active));

                    foreach (Edge e in n.RightEdges)
                        e.Active = false;

                    foreach (Edge e in n.LeftEdges)
                        e.Active = false;

                    Nodes.Remove(n);
                }
            }

            Nodes = BackupNodes;
            Edges = AcyclicEdges;

            foreach (Edge e in Edges)
                e.Active = true;

            return true;
        }

        public bool RemoveTransitiveEdges()
        {
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
            
            return true;
        }

        public bool AssignLayers()
        {
            // This method implements Coffman-Graham layering algorithm.
            
            List<Node> OrderedNodes = Nodes.OrderByDescending(x => x.LeftEdges.Count(e => e.Active)).ToList();
            HashSet<Node> LayeredNodes = new HashSet<Node>();

            Layers.Add(new List<Node>());
            int k = 0;
            int processed = 0;
            while (processed < OrderedNodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.
                
                List<Node> selected = OrderedNodes.Where(x => x.Layer < 0 &&
                    x.RightEdges.Where(e => e.Active).All(e => e.EndNode.Layer >= 0)).ToList();

                Node n = selected.FirstOrDefault(x => x.RightEdges.All(e => e.EndNode.Layer < k) && x.LeftEdges.Count(e => e.Active) > 0);
                if (n == null) n = selected.First();

                if ((GetLayerWidth(k) > MaxWidth) || !n.RightEdges.Where(e => e.Active).All(e => e.EndNode.Layer < k))
                {
                    k++;
                    Layers.Add(new List<Node>());
                }

                n.Layer = k;
                n.X = 1000 - 250 * k;
                Layers[k].Add(n);
                processed++;
            }

            Nodes = new HashSet<Node>(OrderedNodes);

            foreach (List<Node> layer in Layers)
            {
                int y = 0;
                foreach (Node node in layer)
                {
                    node.Y = y;
                    y += 100;
                }
            }

            return true;
        }

        public bool OrderNodes()
        {
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
                    if (n.Y - top.Y < top.Height + 20)
                    {
                        n.Y = top.Y + top.Height + 20;
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

            return false;
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

        public Node Clone()
        {
            return this.MemberwiseClone() as Node;
        }
    }

    public class Edge
    {
        private Graph OwnerGraph;

        public Guid Id;

        public Guid StartId;
        public Guid EndId;

        public Node StartNode;
        public Node EndNode;

        public double EndX;
        public double EndY;

        public bool Active = true;

        public Edge(Guid guid, Guid start, Guid end, double endX, double endY, Graph ownerGraph)
        {
            Id = guid;
            StartId = start;
            EndId = end;
            EndX = endX;
            EndY = endY;
            OwnerGraph = ownerGraph;

            StartNode = OwnerGraph.FindNode(StartId);
            if (StartNode != null)
                StartNode.RightEdges.Add(this);

            EndNode = OwnerGraph.FindNode(EndId);
            if (EndNode != null)
                EndNode.LeftEdges.Add(this);
        }

        public Edge Clone()
        {
            return this.MemberwiseClone() as Edge;
        }
    }
}
