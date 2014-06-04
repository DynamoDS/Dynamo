using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    public class Graph
    {
        public int MaxLayerHeight = 15;
        public double HorizontalNodeDistance = 100;
        public double VerticalNodeDistance = 30;
        public double Infinite = 1000000;

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
                Layers.Add(Nodes.Where(x => x.RightEdges.Count == 0
                    && x.LeftEdges.Count > 0).OrderBy(x => x.Y).ToList());
            else
                Layers.Add(Nodes.OrderBy(x => x.Y).ToList());

            foreach (Node n in Layers.First())
                n.Layer = 0;

            // Label the rest of the nodes based on the number of incoming edges.
            List<Node> OrderedNodes = Nodes.OrderByDescending(
                x => x.LeftEdges.Count(e => e.Active)).ToList();

            bool isFinalLayer = false;
            int currentLayer = 0;
            int processed = Layers.First().Count;

            double layerWidth = 0;
            double previousLayerX = 0;

            while (processed < OrderedNodes.Count)
            {
                // Choose a node with the highest priority (leftmost in the list)
                // such that all the right edges of the is node connected to U.
                
                List<Node> selected = OrderedNodes.Where(x => x.Layer < 0 &&
                    x.RightEdges.All(e => e.EndNode.Layer >= 0)).ToList();

                Node n = selected.FirstOrDefault(x =>
                    x.RightEdges.All(e => e.EndNode.Layer < currentLayer) &&
                    x.LeftEdges.Count(e => e.Active) > 0);

                if (n == null)
                    n = selected.OrderByDescending(x => x.LeftEdges.Count).First();

                // Add a new layer when needed
                if ((Layers[currentLayer].Count >= MaxLayerHeight) ||
                    !n.RightEdges.All(e => e.EndNode.Layer < currentLayer) ||
                    (currentLayer > 0 && n.LeftEdges.Count == 0 && !isFinalLayer))
                {
                    // Horizontal node alignment for the previous layer
                    if (currentLayer > 0)
                        previousLayerX = Layers[currentLayer - 1].First().X;
                    foreach (Node x in Layers[currentLayer])
                        x.X = previousLayerX - layerWidth - HorizontalNodeDistance;
                    
                    currentLayer++;
                    Layers.Add(new List<Node>());

                    layerWidth = 0;

                    if (n.LeftEdges.Count == 0)
                        isFinalLayer = true;
                }

                n.Layer = currentLayer;
                Layers[currentLayer].Add(n);
                processed++;

                if (n.Width > layerWidth)
                    layerWidth = n.Width;
            }

            // Horizontal node alignment for the last (leftmost) layer
            if (currentLayer > 0) previousLayerX = Layers[currentLayer - 1].First().X;
            foreach (Node x in Layers[currentLayer])
                x.X = previousLayerX - layerWidth - HorizontalNodeDistance;

            Nodes = new HashSet<Node>(OrderedNodes);
        }

        /// <summary>
        /// Sugiyama step 3: Node Ordering
        /// This method uses Median heuristic to determine the vertical node
        /// order for each layer.  This includes an implementation to avoid
        /// vertical node overlapping.
        /// </summary>
        public void OrderNodes()
        {
            // Assign temporary vertical position for further processing
            foreach (List<Node> layer in Layers)
            {
                foreach (Node node in layer)
                    node.Y = Infinite;
            }
            double y = 0;
            foreach (Node node in Layers.First())
            {
                node.Y = y;
                y += 80;
            }
            
            List<Node> previousLayer = null;
            foreach (List<Node> layer in Layers)
            {
                if (previousLayer != null)
                {
                    // Get the temporary vertical coordinates from each node's
                    // median outgoing edge

                    foreach (Node n in layer)
                    {
                        List<Edge> neighborEdges = n.RightEdges.OrderBy(x => x.EndY).ToList();

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
                    }
                }

                // Sort the nodes on the layer by its temporary coordinates
                previousLayer = layer.OrderBy(x => x.Y).ToList();
                Node top = null;
                foreach (Node n in previousLayer)
                {
                    // Assign new coordinates to this node's incoming edges
                    int b = 1;
                    foreach (Edge e in n.LeftEdges.OrderBy(x => x.EndY))
                    {
                        e.EndY = n.Y + b;
                        b++;
                    }
                    
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

                    top = n;
                }
            }

            int i = 0;
            while (i < Layers.Count)
            {
                Layers[i] = Layers[i].OrderBy(x => x.Y).ToList();
                i++;
            }
        }

        #endregion

        /// <summary>
        /// To align the top and left bound of the graph at x = 0 and y = 0.
        /// </summary>
        public void NormalizeGraphPosition()
        {
            double offsetX = -Layers.Last().First().X;
            double offsetY = Nodes.OrderBy(x => x.Y).First().Y;

            foreach (List<Node> layer in Layers)
            {
                double maxY = -Infinite;
                foreach (Node n in layer)
                {
                    n.X += offsetX;
                    n.Y += offsetY;

                    if (n.Y >= Infinite + offsetY)
                        n.Y = maxY + VerticalNodeDistance;
                    
                    if (n.Y + n.Height > maxY)
                        maxY = n.Y + n.Height;
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
