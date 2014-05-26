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

        public void AddNode(Guid guid, double width, double height)
        {
            var node = new Node(guid, width, height, this);
            Nodes.Add(node);
        }

        public void AddEdge(Guid guid, Guid start, Guid end)
        {
            var edge = new Edge(guid, start, end, this);
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
            foreach (Edge edge in Edges)
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
                HashSet<Node> selected = new HashSet<Node>(Nodes.Where(n => n.RightEdges.Count == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.LeftEdges)
                    {
                        AcyclicEdges.Add(e);
                        e.StartNode.InactiveRightEdges.Add(e);
                        e.StartNode.RightEdges.Remove(e);
                    }
                    n.InactiveLeftEdges = n.LeftEdges;
                    n.LeftEdges = new HashSet<Edge>();
                }

                // Remove all isolated nodes
                Nodes.ExceptWith(selected);

                // Remove all source nodes
                selected = new HashSet<Node>(Nodes.Where(n => n.LeftEdges.Count == 0));
                foreach (Node n in selected)
                {
                    foreach (Edge e in n.RightEdges)
                    {
                        AcyclicEdges.Add(e);
                        e.EndNode.InactiveLeftEdges.Add(e);
                        e.EndNode.LeftEdges.Remove(e);
                    }
                    n.InactiveRightEdges = n.RightEdges;
                    n.RightEdges = new HashSet<Edge>();
                }

                // Remove all isolated nodes
                Nodes.ExceptWith(selected);

                // if G is not empty then
                if (Nodes.Count > 0)
                {
                    int max = Nodes.Max(x => x.RightEdges.Count - x.LeftEdges.Count);
                    Node n = Nodes.First(x => x.RightEdges.Count - x.LeftEdges.Count == max);

                    AcyclicEdges.UnionWith(n.RightEdges);
                    
                    foreach (Edge e in n.RightEdges)
                        e.EndNode.LeftEdges.Remove(e);
                    
                    foreach (Edge e in n.LeftEdges)
                        e.StartNode.RightEdges.Remove(e);

                    Nodes.Remove(n);
                }
            }

            Nodes = BackupNodes;
            Edges = AcyclicEdges;

            foreach (Node n in Nodes)
            {
                n.LeftEdges.UnionWith(n.InactiveLeftEdges);
                n.RightEdges.UnionWith(n.InactiveRightEdges);
                n.InactiveLeftEdges.Clear();
                n.InactiveRightEdges.Clear();
            }

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
                            e.StartNode.RightEdges.Remove(e);
                            e.EndNode.LeftEdges.Remove(e);
                            Edges.Remove(e);
                        }
                    }
                }
            }
            
            return true;
        }

        public bool AssignLayers()
        {
            // This method implements Coffman-Graham layering algorithm.
            
            List<Node> OrderedNodes = Nodes.OrderByDescending(x => x.LeftEdges.Count).ToList();
            HashSet<Node> LayeredNodes = new HashSet<Node>();

            int layer = 1;
            while (LayeredNodes.Count < OrderedNodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.
                
                Node n = OrderedNodes.First(x => x.Layer == 0 &&
                    x.RightEdges.All(e => LayeredNodes.Contains(e.EndNode)));

                if ((GetLayerWidth(layer) > MaxWidth) || !n.RightEdges.All(e => e.EndNode.Layer < layer))
                    layer++;

                n.Layer = layer;
                LayeredNodes.Add(n);
            }

            Nodes = new HashSet<Node>(OrderedNodes);
            return true;
        }
    }

    public class Node
    {
        private Graph OwnerGraph;

        public Guid Id;

        public double Width;
        public double Height;

        public int Layer = 0;

        public HashSet<Edge> LeftEdges = new HashSet<Edge>();
        public HashSet<Edge> RightEdges = new HashSet<Edge>();

        public HashSet<Edge> InactiveLeftEdges = new HashSet<Edge>();
        public HashSet<Edge> InactiveRightEdges = new HashSet<Edge>();

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

        public Edge(Guid guid, Guid start, Guid end, Graph ownerGraph)
        {
            Id = guid;
            StartId = start;
            EndId = end;
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
